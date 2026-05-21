using Microsoft.AspNetCore.Mvc;
using stock_api.Common;
using stock_api.Common.Settings;
using stock_api.Controllers.Request;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestMailController : Controller
    {
        private readonly EmailService _emailService;
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<TestMailController> _logger;

        public TestMailController(EmailService emailService, SmtpSettings smtpSettings, ILogger<TestMailController> logger)
        {
            _emailService = emailService;
            _smtpSettings = smtpSettings;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendTestMail([FromBody] TestMailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "email 為必填",
                });
            }

            var title = string.IsNullOrWhiteSpace(request.Title)
                ? "Stock API 寄信測試"
                : request.Title;
            var content = string.IsNullOrWhiteSpace(request.Content)
                ? $"<p>這是 stock_api 寄信測試信件。</p><p>寄出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p><p>寄件者：{_smtpSettings.User}</p>"
                : request.Content;

            _logger.LogInformation($"[TestMail] sending test mail to {request.Email}");
            var (success, error) = await _emailService.SendWithResultAsync(title, content, request.Email);

            return Ok(new CommonResponse<dynamic>
            {
                Result = success,
                Message = success ? "寄信成功" : $"寄信失敗：{error}",
                Data = new
                {
                    SmtpServer = _smtpSettings.Server,
                    SmtpPort = _smtpSettings.Port,
                    SmtpUser = _smtpSettings.User,
                    ToEmail = request.Email,
                    Title = title,
                    Error = error,
                }
            });
        }
    }
}
