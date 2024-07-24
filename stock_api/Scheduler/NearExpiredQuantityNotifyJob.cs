
using Org.BouncyCastle.Asn1.Ocsp;
using Quartz;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using System.Text;

namespace stock_api.Scheduler
{
    public class NearExpiredQuantityNotifyJob : IJob
    {
        private readonly WarehouseProductService _warehouseProductService;
        private readonly EmailService _emailService;
        private readonly StockInService _stockInService;
        private readonly MemberService _memberService;

        public NearExpiredQuantityNotifyJob(WarehouseProductService warehouseProductService,EmailService emailService, StockInService stockInService,MemberService memberService)
        {
            _warehouseProductService = warehouseProductService;
            _emailService = emailService;
            _stockInService = stockInService;
            _memberService = memberService;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            var allActiveProduct = _warehouseProductService.GetAllActiveProducts();
            var distinctCompIdList = allActiveProduct.Select(p=>p.CompId).Distinct().ToList();
            distinctCompIdList.ForEach(compId =>
            {
                List<NearExpiredProductVo> nearExpiredProductVoList =  _stockInService.GetNearExpiredProductList(compId, today);
                if (nearExpiredProductVoList.Count > 0)
                {
                    string emailTitle = "以下已入庫的品項即將過期,請盡快使用";
                    string emailBody = GenerateHtmlString(nearExpiredProductVoList);
                    List<WarehouseMember> receiverList = _memberService.GetAllMembersOfComp(compId).Where(m => m.IsActive == true).ToList();
                    List<string> effectiveEmailList = receiverList.Where(r => !string.IsNullOrEmpty(r.Email)).Select(r => r.Email).ToList();
                    effectiveEmailList.ForEach(effectiveEmail => _emailService.SendAsync(emailTitle, emailBody, effectiveEmail));
                }
            });


        }

        public  static string GenerateHtmlString(List<NearExpiredProductVo> nearExpiredProductVoList)
        {
            var stringBuilder = new StringBuilder();
            

            foreach (var nearExpiredProductVo in nearExpiredProductVoList)
            {
                stringBuilder.AppendLine($"<h1>品項名:{nearExpiredProductVo.ProductName}");
                stringBuilder.AppendLine("<br/>");
                stringBuilder.AppendLine($"<h1>品項編號:{nearExpiredProductVo.ProductCode}</h1>");
                stringBuilder.AppendLine("<table border='1'>");
                stringBuilder.AppendLine("<thead><tr><th>批號</th><th>批次</th><th>有效期限</th><th>幾天前提醒</th><th>入庫數量</th><th>已出庫數量</th></tr></thead>");
                stringBuilder.AppendLine("<tbody>");
                foreach (var inStockItem in nearExpiredProductVo.InStockItemList)
                {
                    stringBuilder.AppendLine("<tr>");
                    stringBuilder.AppendLine($"<td>{inStockItem.LotNumber}</td>");
                    stringBuilder.AppendLine($"<td>{inStockItem.LotNumberBatch}</td>");
                    stringBuilder.AppendLine($"<td>{inStockItem.ExpirationDate}</td>");
                    stringBuilder.AppendLine($"<td>{nearExpiredProductVo.PreDeadline}</td>");
                    stringBuilder.AppendLine($"<td>{inStockItem.InStockQuantity}</td>");
                    stringBuilder.AppendLine($"<td>{inStockItem.OutStockQuantity}</td>");
                    stringBuilder.AppendLine("</tr>");
                }
                stringBuilder.AppendLine("</tbody>");
                stringBuilder.AppendLine("</table>");
                stringBuilder.AppendLine("<br/><br/>");
            }
            return stringBuilder.ToString();
        }
    }
}
