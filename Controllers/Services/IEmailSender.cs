// File: Services/IEmailSender.cs
namespace HeThongTimViec.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}