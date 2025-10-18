using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace HeThongTimViec.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mailSettings = _configuration.GetSection("MailSettings");
            var smtpServer = mailSettings["Host"];
            var portValue = mailSettings["Port"];
            if (string.IsNullOrEmpty(portValue))
            {
                throw new InvalidOperationException("SMTP port is not configured.");
            }
            var smtpPort = int.Parse(portValue);
            var smtpUsername = mailSettings["Mail"];
            var smtpPassword = mailSettings["Password"];
            var fromEmail = mailSettings["Mail"];
            var fromName = mailSettings["DisplayName"];

            if (string.IsNullOrEmpty(fromEmail))
            {
                throw new InvalidOperationException("Sender email address is not configured.");
            }

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Lỗi khi gửi email: {ex.Message}", ex);
                }
            }
        }
    }
}