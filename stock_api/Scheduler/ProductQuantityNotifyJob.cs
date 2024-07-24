
using Org.BouncyCastle.Asn1.Ocsp;
using Quartz;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using System.Text;

namespace stock_api.Scheduler
{
    public class ProductQuantityNotifyJob :IJob
    {
        private readonly WarehouseProductService _warehouseProductService;
        private readonly EmailService _emailService;
        private readonly PurchaseService _purchaseService;
        private readonly MemberService _memberService;

        public ProductQuantityNotifyJob(WarehouseProductService warehouseProductService,EmailService emailService, PurchaseService purchaseService,MemberService memberService)
        {
            _warehouseProductService = warehouseProductService;
            _emailService = emailService;
            _purchaseService = purchaseService;
            _memberService = memberService;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            var notifyProductQuantityList = await _warehouseProductService.FindAllProductQuantityNotifyList();
            Dictionary<string,List<NotifyProductQuantity>> compIdAndNotifyProductQuantityMap = notifyProductQuantityList
            .GroupBy(npq => npq.CompId)
            .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var keyPair in compIdAndNotifyProductQuantityMap)
            {
                var compId = keyPair.Key;
                var notifyProductList = keyPair.Value??new List<NotifyProductQuantity>();
                string emailTitle = "庫存品項不足通知";
                string emailBody = GenerateHtmlString(notifyProductList);
                List<WarehouseMember> receiverList = _memberService.GetAllMembersOfComp(compId).Where(m => m.IsActive == true).ToList();
                List<string> effectiveEmailList = receiverList.Where(r => !string.IsNullOrEmpty(r.Email)).Select(r => r.Email).ToList();
                effectiveEmailList.ForEach(effectiveEmail => _emailService.SendAsync(emailTitle, emailBody, effectiveEmail));
            }
        }

        public  static string GenerateHtmlString(List<NotifyProductQuantity> notifyProductQuantityList)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("<h2>以下品項庫存不足(庫存量<最大安庫量-正在處理中訂單量)</h2>");
            stringBuilder.AppendLine("<table border='1'>");
            stringBuilder.AppendLine("<tr><th>品項名稱</th><th>品項編碼</th><th>庫存量</th><th>最大安庫量</th><th>最小安庫量</th><th>處理中訂單量</th></tr>");

            foreach (var item in notifyProductQuantityList)
            {
                if (item.MaxSafeQuantity.HasValue && item.InStockQuantity < item.MaxSafeQuantity.Value - item.InProcessingQrderQuantity)
                {
                    stringBuilder.AppendLine("<tr>");
                    stringBuilder.AppendLine($"<td>{item.ProductName}</td>");
                    stringBuilder.AppendLine($"<td>{item.ProductCode}</td>");
                    stringBuilder.AppendLine($"<td>{item.InStockQuantity}</td>");
                    stringBuilder.AppendLine($"<td>{item.MaxSafeQuantity}</td>");
                    stringBuilder.AppendLine($"<td>{item.SafeQuantity}</td>");
                    stringBuilder.AppendLine($"<td>{item.InProcessingQrderQuantity}</td>");
                    stringBuilder.AppendLine("</tr>");
                }
            }

            stringBuilder.AppendLine("</table>");

            return stringBuilder.ToString();
        }
    }
}
