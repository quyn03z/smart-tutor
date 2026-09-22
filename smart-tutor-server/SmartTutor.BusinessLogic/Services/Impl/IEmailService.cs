using System.Threading.Tasks;

namespace SmartTutor.BusinessLogic.Services.Impl
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendResetPasswordEmailAsync(string toEmail, string resetLink, string fullName);
    }
}
