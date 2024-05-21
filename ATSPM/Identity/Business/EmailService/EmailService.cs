using ATSPM.Domain.Services;
using System.Net.Mail;

namespace Identity.Business.EmailSender
{
    public class EmailService 
    {
        private readonly IEmailService mailService;

        public EmailService(IEmailService mailService)
        {
            this.mailService = mailService;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("dlowe@avenueconsultants.com"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            return mailService.SendEmailAsync(mailMessage);
        }
    }
}
