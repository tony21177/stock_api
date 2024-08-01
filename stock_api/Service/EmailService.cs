using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using stock_api.Common.Constant;
using stock_api.Common.Settings;
using stock_api.Models;


public class EmailService
{
    private readonly StockDbContext _dbContext;
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;
    public EmailService(SmtpSettings smtpSettings, ILogger<EmailService> logger,StockDbContext stockDbContext)
    {
        _smtpSettings = smtpSettings;
        _logger = logger;
        _dbContext = stockDbContext;
    }

    public async Task SendAsync(string title, string content, string email)
    {
       var message = new MimeMessage();
        message.From.Add(new MailboxAddress( "庫存系統", _smtpSettings.User));
        message.To.Add(new MailboxAddress("庫存系統成員", email));
        message.Subject = title;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = content
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            client.Connect(_smtpSettings.Server, _smtpSettings.Port, true);
            client.Authenticate(_smtpSettings.User, _smtpSettings.Password);
            await client.SendAsync(message);
            //_logger.LogInformation($"sendResult {sendResult}");
        }
        catch (Exception ex)
        {
            // Log the exception or handle it accordingly
            _logger.LogError($"An error occurred while sending an email to {email}: {ex.Message}");
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    public void AddEmailNotify(EmailNotify newEmailNotify)
    {
        _dbContext.EmailNotifies.Add(newEmailNotify);
        _dbContext.SaveChanges();
    }

    public void UpdateEmailNotifyIsSendByIdList(List<int> idList)
    {
        _dbContext.EmailNotifies.Where(n => idList.Contains(n.Id)).ExecuteUpdate(n => n.SetProperty(n => n.IsSend, true));
        _dbContext.SaveChanges();
        return;
    }

    public Dictionary<string, List<EmailNotify>> GetNormalPurchaseListToSend()
    {
        List<EmailNotify> waitingToNotifyListForPurchase = _dbContext.EmailNotifies.Where(e=>e.Type==CommonConstants.EmailNotifyType.PURCHASE&&e.IsSend==false).ToList();
        Dictionary<string, List<EmailNotify>> emailNotifyDictionary = waitingToNotifyListForPurchase
            .Where(e => !string.IsNullOrEmpty(e.Email))
            .GroupBy(e => e.Email)
            .ToDictionary(g => g.Key, g => g.ToList());
        return emailNotifyDictionary;
    }
}
