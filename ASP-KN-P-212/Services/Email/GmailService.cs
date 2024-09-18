using System.Net;
using System.Net.Mail;

namespace ASP_KN_P_212.Services.Email
{
    public class GmailService(IConfiguration configuration) : IEmailService
    {
        private readonly IConfiguration _configuration = configuration;
        
        public void Send(MailMessage mailMessage)
        {
            IConfigurationSection? smtp = _configuration.GetSection("smtp");
            String? host = smtp.GetSection("host").Value;
            String? email = smtp.GetSection("email").Value;
            String? password = smtp.GetSection("password").Value;
            Boolean ssl = smtp.GetValue<Boolean>("ssl");
            Int32 port = smtp.GetValue<Int32>("port");
            if (host == null || email == null || password == null)
            {
                throw new Exception("Configuration error");
            }
            using SmtpClient smtpClient = new()
            {
                Host = host,
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
            mailMessage.From = new MailAddress(email);
            if (String.IsNullOrEmpty(mailMessage.Subject))
            {
                mailMessage.Subject = "ASP-212 site notification";
            }
            smtpClient.Send(mailMessage);
        }
    }
}
/*  // базові можливості
smtpClient.Send(
    from: email,
    recipients: "denniksam@gmail.com",
    subject: "ASP-212 site message",
    body: "Hello from the site!");
*/