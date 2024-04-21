using stock_api.Models;
using Microsoft.EntityFrameworkCore;
using stock_api.Controllers.Request;
using System.Transactions;

namespace stock_api.Service
{
    public class AuthLayerService
    {
        private readonly StockDbContext _dbContext;
        private readonly ILogger<PurchaseService> _logger;

        public AuthLayerService(StockDbContext dbContext, ILogger<PurchaseService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public List<WarehouseAuthlayer> GetAllAuthlayers(string compId)
        {
            return _dbContext.WarehouseAuthlayers.Where(al => al.CompId == compId).ToList();
        }
        public WarehouseAuthlayer? GetByAuthValue(short authValue, string compId)
        {
            return _dbContext.WarehouseAuthlayers.Where(authLayer => authLayer.AuthValue == authValue && authLayer.CompId == compId).FirstOrDefault();
        }

        public List<WarehouseAuthlayer> UpdateAuthlayers(List<UpdateAuthlayerRequest> requestList)
        {
            var updatedAuthLayers = new List<WarehouseAuthlayer>();
            requestList.ForEach(request =>
            {
                var existingAuthLayer = _dbContext.WarehouseAuthlayers.Find(request.AuthId);
                if (existingAuthLayer != null)
                {
                    // 使用 SetValues 來只更新不為 null 的屬性
                    _dbContext.Entry(existingAuthLayer).CurrentValues.SetValues(request);
                    updatedAuthLayers.Add(existingAuthLayer);
                }
            });
            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return updatedAuthLayers;
        }

        public WarehouseAuthlayer AddAuthlayer(WarehouseAuthlayer newAuthlayer)
        {
            _dbContext.WarehouseAuthlayers.Add(newAuthlayer);
            _dbContext.SaveChanges(true);
            return newAuthlayer;
        }

        public void DeleteAuthLayer(int id)
        {
            var authLayerToDelete = new WarehouseAuthlayer { AuthId = id };
            // 將實體的狀態設置為 'Deleted'
            _dbContext.Entry(authLayerToDelete).State = EntityState.Deleted;

            // 將更改應用到資料庫
            _dbContext.SaveChanges();
            return;
        }

        public bool ResetAllAuthLayer(string compId)
        {
            using var scope = new TransactionScope();
            try
            {
                _dbContext.WarehouseAuthlayers.Where(auth=>auth.CompId==compId).ExecuteDelete();
                List<WarehouseAuthlayer> warehouseAuthlayers = new();
                for(int i = 1; i <= 9; i+=2)
                {
                    string authDescription="",authName="";
                    short authValue=0;
                    bool isApplyItemManage = true,
                        isGroupManage = true,
                        isInBoundManage = true,
                        isInventoryManage = true,
                        isItemManage = true,
                        isMemberManage = true,
                        isOutBoundManage = true,
                        isRestockManage = true,
                        isVerifyManage = true;
                    switch (i) {
                        case 1:
                            authDescription = "適用實驗室主管";
                            authName = "最高層級";
                            authValue = 1;
                            break;
                        case 3:
                            authDescription = "適用管理階層";
                            authName = "第一層級";
                            authValue = 3;
                            break;
                        case 5:
                            authDescription = "適用部門主管";
                            authName = "第二層級";
                            authValue = 5;
                            break;
                        case 7:
                            authDescription = "適用一般醫檢師";
                            authName = "第三層級";
                            authValue = 7;
                            isMemberManage= false;
                            break;
                        case 9:
                            authDescription = "適用行政人員";
                            authName = "第四層級";
                            authValue = 9;
                            isMemberManage = false;
                            break;

                    }

                    var newAuthlayer = new WarehouseAuthlayer
                    {
                        AuthDescription = authDescription,
                        AuthName = authName,
                        AuthValue = authValue,
                        CompId = compId,
                        IsApplyItemManage = isApplyItemManage,
                        IsGroupManage = isGroupManage,
                        IsInBoundManage = isInBoundManage,
                        IsInventoryManage = isInBoundManage,
                        IsItemManage = isItemManage,
                        IsMemberManage = isMemberManage,
                        IsOutBoundManage = isOutBoundManage,
                        IsRestockManage = isRestockManage,
                        IsVerifyManage = isVerifyManage
                    };
                    warehouseAuthlayers.Add(newAuthlayer);
                }
                _dbContext.WarehouseAuthlayers.AddRange(warehouseAuthlayers);
                _dbContext.SaveChanges();
                scope.Complete();
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[ResetAllAuthLayer]：{msg}", ex);
                return false;
            }
        }
    }
}
