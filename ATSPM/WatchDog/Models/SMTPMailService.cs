using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace WatchDog.Models
{
    public class SMTPMailService : IMailService
    {
        private readonly ILogger<SMTPMailService> logger;
        SmtpClient smtp;
        public SMTPMailService(string emailServer, int port, bool enableSsl, string userName, string password, ILogger<SMTPMailService> logger)
        {
            this.logger = logger;
            smtp = new SmtpClient();
            try
            {
                logger.LogInformation($"Connecting to {emailServer} on port {port} with SSL: {enableSsl}");
                smtp.Connect(emailServer, port, enableSsl == true ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                logger.LogInformation($"Connected to {emailServer} on port {port} with SSL: {enableSsl}");
                logger.LogInformation($"Authenticating with {userName}");
                smtp.Authenticate(userName, password);
                logger.LogInformation($"Authenticated with {userName}");
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        public async Task<bool> SendEmailAsync(string from, List<ApplicationUser> users, string subject, string body)
        {
            if (smtp is null)
            {
                throw new ArgumentNullException(nameof(smtp));
            }
            var message = new MimeMessage();
            AddUsersToMessage(users, message);
            message.Subject = subject;
            message.From.Add(new MailboxAddress("ATSPM Admin", from));
            logger.LogInformation($"Sender set to: {from}");
            message.Body = new TextPart("html")
            {
                Text = body
            };
            try
            {
                logger.LogInformation($"Sending Email to: {message.To} \nMessage text: {message.Body} \n");
                var result = await smtp.SendAsync(message);
                logger.LogInformation($"Email Sent Successfully to: {message.To}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }

        }

        private void AddUsersToMessage(List<ApplicationUser> users, MimeMessage? message)
        {
            try
            {
                foreach (var user in users)
                {
                    AddUserToMessage(message, user.Email, user.FullName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding users to email");
                throw;
            }
        }

        private void AddUserToMessage(MimeMessage message, string email, string name)
        {
            message.To.Add(new MailboxAddress(name, email));
            logger.LogInformation($"{email} - {name} added to email");
        }
    }
}
