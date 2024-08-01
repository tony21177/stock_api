
using Org.BouncyCastle.Asn1.Ocsp;
using Quartz;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using System.Text;

namespace stock_api.Scheduler
{
    public class PurchaseNotifyJob : IJob
    {
        private readonly EmailService _emailService;

        public PurchaseNotifyJob(EmailService emailService)
        {
            _emailService = emailService;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            Dictionary<string,List<EmailNotify>> emailAndNotifyListMap = _emailService.GetNormalPurchaseListToSend();

            foreach (var keyPair in emailAndNotifyListMap)
            {
                var email = keyPair.Key;
                var notifyList = keyPair.Value??new List<EmailNotify>();
                if (notifyList.Count > 0)
                {
                    string emailTitle = notifyList[0].Title;
                    List<string> emailContentList = notifyList.Select(x => x.Content).ToList();   
                    string emailContent = string.Join("<br/>", emailContentList);
                    _emailService.SendAsync(email, emailContent, email);
                }
            }
        }
    }
}
