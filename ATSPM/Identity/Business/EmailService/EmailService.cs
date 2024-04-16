using System.Net.Mail;
using System.Net;

namespace Identity.Business.EmailSender
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;

        public EmailService()
        {
            // Configure your SMTP settings
            _smtpClient = new SmtpClient
            {
                Host = "smtp.freesmtpservers.com",
                Port = 25,
                Credentials = new NetworkCredential(),
                //EnableSsl = true,
            };
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("support@avenueconsultants.com"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            return _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
