using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTutor.BusinessLogic.Services.Impl;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.TryParse(_configuration["Smtp:Port"], out int p) ? p : 587;
            var enableSsl = bool.TryParse(_configuration["Smtp:EnableSsl"], out bool ssl) ? ssl : true;
            var username = _configuration["Smtp:Username"] ?? "";
            var password = _configuration["Smtp:Password"] ?? "";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(username, "SmartTutor System"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw new InvalidOperationException($"Không thể gửi email đến {toEmail}. Vui lòng thử lại sau.", ex);
            }
        }

        public async Task SendResetPasswordEmailAsync(string toEmail, string resetLink, string fullName)
        {
            var displayName = string.IsNullOrWhiteSpace(fullName) ? "Quý Thầy/Cô" : fullName;
            var subject = "[SmartTutor] Hướng dẫn khôi phục mật khẩu tài khoản";

            var htmlBody = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f1f5f9; margin: 0; padding: 20px; }}
        .container {{ max-width: 560px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.08); }}
        .header {{ background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%); padding: 32px 24px; text-align: center; color: #ffffff; }}
        .brand {{ font-size: 24px; font-weight: 800; color: #ffffff; letter-spacing: -0.5px; }}
        .brand span {{ color: #38bdf8; }}
        .body {{ padding: 32px 28px; color: #334155; line-height: 1.6; font-size: 15px; }}
        .btn {{ display: inline-block; background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); color: #ffffff !important; padding: 14px 28px; border-radius: 8px; font-weight: 700; text-decoration: none; margin: 24px 0; font-size: 15px; box-shadow: 0 4px 12px rgba(37,99,235,0.3); }}
        .notice {{ background-color: #f8fafc; border-left: 4px solid #2563eb; padding: 14px 18px; border-radius: 6px; font-size: 13.5px; color: #64748b; margin-top: 20px; }}
        .footer {{ background-color: #f8fafc; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""brand"">Smart<span>Tutor</span></div>
        </div>
        <div class=""body"">
            <p>Xin chào <strong>{displayName}</strong>,</p>
            <p>Hệ thống SmartTutor vừa nhận được yêu cầu đặt lại mật khẩu cho tài khoản đăng ký với email <strong>{toEmail}</strong>.</p>
            <p>Để tạo mật khẩu mới, vui lòng nhấn vào nút bên dưới:</p>
            <div style=""text-align: center;"">
                <a href=""{resetLink}"" class=""btn"">Đặt Lại Mật Khẩu Ngay</a>
            </div>
            <div class=""notice"">
                <strong>Lưu ý:</strong>
                <ul>
                    <li>Liên kết này chỉ có hiệu lực trong vòng <strong>15 phút</strong>.</li>
                    <li>Nếu bạn không yêu cầu hành động này, vui lòng bỏ qua email và mật khẩu của bạn vẫn được giữ nguyên an toàn.</li>
                </ul>
            </div>
            <p style=""margin-top: 20px; font-size: 13px; color: #94a3b8; word-break: break-all;"">
                Hoặc sao chép đường dẫn sau vào trình duyệt:<br>
                <a href=""{resetLink}"" style=""color: #2563eb;"">{resetLink}</a>
            </p>
        </div>
        <div class=""footer"">
            &copy; SmartTutor - Hệ thống Quản lý Gia sư & Tự động hóa học phí thông minh.
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }
    }
}
