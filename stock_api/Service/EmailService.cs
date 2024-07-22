using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using stock_api.Common.Settings;
using System.Threading.Tasks;
using stock_api.Service;
using System.Text;

public class EmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;
    public EmailService(SmtpSettings smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings;
        _logger = logger;
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
            var sendResult = client.Send(message);
            _logger.LogInformation($"sendResult {sendResult}");
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
}
