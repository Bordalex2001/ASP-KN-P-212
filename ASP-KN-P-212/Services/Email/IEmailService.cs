using System.Net.Mail;

namespace ASP_KN_P_212.Services.Email
{
    public interface IEmailService
    {
        void Send(MailMessage mailMessage);
    }
}