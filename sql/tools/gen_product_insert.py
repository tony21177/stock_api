#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
從「庫存總覽表」匯出的 Excel 產生 warehouse_product 的 INSERT SQL。

用法：
  1. 安裝套件：python -m pip install pandas openpyxl
  2. 編輯下面的 JOBS（每個檔對應一個 CompId + 要掛的組別名稱）與 DB 連線設定。
  3. 執行：python sql/tools/gen_product_insert.py
  4. 產出檔：sql/tools/_generated_product_insert.sql
  5. 先 dry-run 驗證（會 rollback，不會真的寫入）：
       見檔尾註解的指令。

腳本會自動連 DB：
  - 用供應商「名稱」查 supplier.Id（查不到 → 該筆供應商留 NULL 並列警告）
  - 用組別「名稱」+ CompId 查 warehouse_group.GroupId（查不到 → 中止並提示）
  - ProductId 採 {CompId}-{編碼} 格式（與本專案慣例一致）
  - ProductCode 直接沿用 Excel 的「編碼」欄

欄位對應、是/否、QcType、寄送類型等轉換規則見下方程式。
"""
import pandas as pd, re, math, subprocess, sys, os

# ============ 設定區（下次重用改這裡） ============
MYSQL = r"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
DB = dict(host="127.0.0.1", user="root", password="root", db="stock")

# 每個 Excel 對應一家：compId / 顯示用 compName / 要掛的組別名稱（需已存在於 warehouse_group）
JOBS = [
    dict(file=r"彰化醫院院區｜醫事檢驗科 - 新增品項 - 20260615.xlsx",
         compId="68c05e1d-c6f5-498a-8359-d961359e5875",
         compName="彰化醫院檢驗科",
         groupName="二樓代檢請購組(不轉訂單)"),
    dict(file=r"金萬林｜金萬林 - 新增品項 - 20260615.xlsx",
         compId="63c0dbc4-fdca-47e8-b084-5880fb596de2",
         compName="金萬林",
         groupName="結核菌代檢"),
]
OUT = os.path.join(os.path.dirname(__file__), "_generated_product_insert.sql")

# 缺少的供應商是否自動產生 INSERT（Id 接在 supplier 最大 Id 之後遞增）
AUTO_CREATE_MISSING_SUPPLIER = True
# 新供應商掛在哪個 CompId（供應商主檔慣例掛在得標廠商 OWNER，預設＝金萬林）
NEW_SUPPLIER_COMPID = "63c0dbc4-fdca-47e8-b084-5880fb596de2"
# =================================================

def query(sql):
    """執行查詢，回傳 list[tuple]（以 tab 分隔，跳過標題列）。"""
    p = subprocess.run(
        [MYSQL, f"-h{DB['host']}", f"-u{DB['user']}", f"-p{DB['password']}",
         DB["db"], "--default-character-set=utf8mb4", "-N", "-B", "-e", sql],
        capture_output=True, text=True, encoding="utf-8")
    if p.returncode != 0:
        sys.exit(f"DB 查詢失敗：{p.stderr}")
    return [line.split("\t") for line in p.stdout.splitlines() if line]

# ---- 值轉換 ----
def num(v):
    if v is None or (isinstance(v, float) and math.isnan(v)): return None
    m = re.match(r"^-?[\d.]+", str(v).strip())
    return float(m.group(0)) if m else None

def s(v):
    if v is None or (isinstance(v, float) and math.isnan(v)): return ""
    return str(v).strip()

def q(v):    return "'" + s(v).replace("\\", "\\\\").replace("'", "\\'") + "'"
def qn(v):
    n = num(v); return "NULL" if n is None else (str(int(n)) if n == int(n) else str(n))
def qi(v):
    n = num(v); return "NULL" if n is None else str(int(n))
def yn(v):   return "1" if s(v) == "是" else "0"

QC = {"批次": "LOT_NUMBER_BATCH", "批號": "LOT_NUMBER", "無": "NONE", "": "NONE"}
def qc(v):    return QC.get(s(v), "NONE")
def deliv(v): t = s(v); return "OWNER" if ("得標" in t or t == "OWNER") else "VENDOR"

# Excel「庫存總覽表」欄位位置（0-based），對應到 header 列
COLS = ["CompId","ProductId","ProductCode","CompName","ProductModel","ProductName","ProductCategory",
        "Unit","PackageWay","ProductSpec","GroupIds","GroupNames","DefaultSupplierID","DefaultSupplierName",
        "ManufacturerId","ManufacturerName","OpenedSealName","StockLocation","InStockQuantity","SafeQuantity",
        "MaxSafeQuantity","Manager","IsNeedAcceptProcess","QcType","IsPrintSticker","IsActive","DeadlineRule",
        "OpenDeadline","PreOrderDays","AllowReceiveDateRange","PreDeadline","Weight","DeliverFunction",
        "DeliverRemarks","SavingFunction","Delievery","UDISerialCode","UnitConversion","ProductMachine",
        "LotNumberBatch","LotNumber","ProductRemarks"]

def main():
    supplier_map = {name: sid for sid, name in query("SELECT Id,Name FROM supplier WHERE Name IS NOT NULL")}

    # 先把每個 job 的 Excel 讀進來、解析組別，並收集所有出現過的供應商名稱
    jobs = []
    seen_sup = set()
    for j in JOBS:
        grp = query(f"SELECT GroupId FROM warehouse_group WHERE CompId='{j['compId']}' AND GroupName='{j['groupName']}'")
        if not grp:
            sys.exit(f"找不到組別「{j['groupName']}」於 CompId={j['compId']}，請先建立或修正設定。")
        df = pd.read_excel(j["file"], header=None)
        hr = df.index[df[0] == "編碼"][0]               # 找標題列
        data = df.iloc[hr + 1:]
        for _, r in data.iterrows():
            if re.match(r"^\d+$", s(r[0])) and s(r[6]):
                seen_sup.add(s(r[6]))
        jobs.append(dict(job=j, data=data, group_id=grp[0][0]))

    # 找出缺少的供應商
    missing_sup = {n for n in seen_sup if n not in supplier_map}
    new_supplier_sql = ""
    if missing_sup and AUTO_CREATE_MISSING_SUPPLIER:
        next_id = int(query("SELECT COALESCE(MAX(Id),0)+1 FROM supplier")[0][0])
        lines = []
        for name in sorted(missing_sup):
            supplier_map[name] = str(next_id)            # 加入對應，供品項列引用
            lines.append(f"  ({next_id},{q(NEW_SUPPLIER_COMPID)},'',{q(name)},1)")
            next_id += 1
        new_supplier_sql = ("-- 自動新增缺少的供應商（Code 留空，需要時再補）\n"
                            "INSERT INTO supplier (Id,CompId,Code,Name,IsActive) VALUES\n"
                            + ",\n".join(lines) + ";\n\n")
        missing_sup = set()                              # 已處理

    out = open(OUT, "w", encoding="utf-8")
    if new_supplier_sql:
        out.write(new_supplier_sql)

    for jd in jobs:
        j, data, group_id = jd["job"], jd["data"], jd["group_id"]
        out.write(f"-- ===== {j['compName']} ({j['compId']}) 掛到「{j['groupName']}」=====\n")
        out.write("INSERT INTO warehouse_product\n  (" + ",".join(COLS) + ")\nVALUES\n")
        rows = []
        for _, r in data.iterrows():
            code = s(r[0])
            if not re.match(r"^\d+$", code):            # 跳過空白/非資料列
                continue
            supname = s(r[6])
            supid = "NULL"
            if supname:
                if supname in supplier_map: supid = supplier_map[supname]
                else: missing_sup.add(supname)
            rows.append("  (" + ",".join([
                q(j["compId"]), q(f"{j['compId']}-{code}"), q(code), q(j["compName"]),
                q(r[1]), q(r[2]), q(r[14]), q(r[4]), q(r[5]), q(r[9]),
                q(group_id), q(j["groupName"]), supid, q(supname),
                "''", q(r[15]), q(r[16]), q(r[7]),
                qn(r[3]), qn(r[17]), qn(r[18]),
                q(r[21]), yn(r[22]), q(qc(r[23])), yn(r[24]), yn(r[25]),
                qi(r[27]), qi(r[30]), qi(r[31]), qi(r[32]), qi(r[33]),
                q(r[34]), q(r[35]), q(r[36]), q(r[37]), q(deliv(r[38])),
                q(r[39]), qn(r[40]), q(r[12]), q(r[10]), q(r[11]), q(r[13]),
            ]) + ")")
        out.write(",\n".join(rows) + ";\n\n")
    out.close()

    if new_supplier_sql:
        print("＋ 已自動產生缺少供應商的 INSERT（Id 接在 supplier 最大 Id 之後）")
    if missing_sup:
        print("⚠ 下列供應商在 supplier 表查無，對應筆數的 DefaultSupplierID 已留 NULL：", missing_sup)
        print("  （AUTO_CREATE_MISSING_SUPPLIER=True 可自動產生其 INSERT）")
    print("已產生：", OUT)

if __name__ == "__main__":
    main()
