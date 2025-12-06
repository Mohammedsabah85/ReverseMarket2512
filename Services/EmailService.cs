using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ReverseMarket.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string message, string? fromName = null);
        Task<bool> SendContactFormEmailAsync(string name, string email, string phone, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody, string? fromName = null)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                // التحقق من وجود الإعدادات
                var smtpServer = emailSettings["SmtpServer"];
                var smtpUsername = emailSettings["SmtpUsername"];
                var smtpPassword = emailSettings["SmtpPassword"];

                if (string.IsNullOrEmpty(smtpServer) ||
                    string.IsNullOrEmpty(smtpUsername) ||
                    string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("Email settings are not configured properly");
                    return false;
                }

                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    fromName ?? emailSettings["FromName"] ?? "ReverseMarket",
                    smtpUsername // استخدم نفس البريد الإلكتروني المستخدم للمصادقة
                ));

                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = messageBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // تفعيل تسجيل التفاصيل للتصحيح
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                _logger.LogInformation($"Attempting to connect to SMTP server: {smtpServer}");

                // الاتصال بالسيرفر
                await client.ConnectAsync(
                    smtpServer,
                    int.Parse(emailSettings["SmtpPort"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                _logger.LogInformation("Connected to SMTP server successfully");

                // المصادقة
                await client.AuthenticateAsync(smtpUsername, smtpPassword);

                _logger.LogInformation("Authenticated successfully");

                // إرسال الرسالة
                await client.SendAsync(message);

                _logger.LogInformation($"Email sent successfully to {toEmail}");

                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }

                return false;
            }
        }

        public async Task<bool> SendContactFormEmailAsync(string name, string email, string phone, string subject, string message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var adminEmail = emailSettings["AdminEmail"] ?? emailSettings["SmtpUsername"];

            if (string.IsNullOrEmpty(adminEmail))
            {
                _logger.LogError("Admin email is not configured");
                return false;
            }

            var htmlMessage = $@"
                <!DOCTYPE html>
                <html dir='rtl' lang='ar'>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f8f9fa; padding: 20px; }}
                        table {{ width: 100%; border-collapse: collapse; }}
                        td {{ padding: 10px; border-bottom: 1px solid #ddd; }}
                        .label {{ background-color: #e9ecef; font-weight: bold; width: 150px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>رسالة جديدة من نموذج الاتصال</h2>
                        </div>
                        <div class='content'>
                            <table>
                                <tr>
                                    <td class='label'>الاسم:</td>
                                    <td>{name}</td>
                                </tr>
                                <tr>
                                    <td class='label'>البريد الإلكتروني:</td>
                                    <td><a href='mailto:{email}'>{email}</a></td>
                                </tr>
                                <tr>
                                    <td class='label'>رقم الهاتف:</td>
                                    <td>{phone ?? "غير مُحدد"}</td>
                                </tr>
                                <tr>
                                    <td class='label'>الموضوع:</td>
                                    <td>{subject}</td>
                                </tr>
                                <tr>
                                    <td class='label' colspan='2'>الرسالة:</td>
                                </tr>
                                <tr>
                                    <td colspan='2' style='padding: 15px; background-color: white;'>
                                        {message.Replace("\n", "<br>")}
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div class='footer'>
                            <p>تم إرسال هذه الرسالة من نموذج الاتصال في موقع السوق العكسي</p>
                            <p>التاريخ: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(adminEmail, $"رسالة اتصال جديدة: {subject}", htmlMessage, name);
        }
    }
}