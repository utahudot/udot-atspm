using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Configuration;
using ATSPM.Domain.Services;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.EmailServices
{
    public class SmtpEmailService : ServiceObjectBase, IEmailService
    {
        private readonly EmailConfiguration _options;
        private readonly ILogger _logger;

        public SmtpEmailService(IOptionsSnapshot<EmailConfiguration> options, ILogger<SmtpEmailService> logger) : base(true)
        {
            _options = options?.Get(this.GetType().Name) ?? options?.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> SendEmailAsync(MailMessage message)
        {
            _logger.LogDebug("SendEmail message: {message}", message);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(message.From.DisplayName, message.From.Address));
            email.To.AddRange(message.To.Select(s => new MailboxAddress(s.DisplayName, s.Address)).ToList());
            email.Subject = message.Subject;

            if (message.IsBodyHtml)
            {
                email.Body = new TextPart(TextFormat.Html) { Text = message.Body };
            }
            else
            {
                email.Body = new TextPart(TextFormat.Plain) { Text = message.Body };
            }

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    await smtp.ConnectAsync(_options.Host, _options.Port, _options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                    await smtp.AuthenticateAsync(_options.UserName, _options.Password);

                    if (smtp.IsConnected && smtp.IsAuthenticated)
                    {
                        _logger.LogDebug("SendEmail sending: {email}", email);

                        var response = await smtp.SendAsync(email);

                        _logger.LogInformation("SendEmail response: {response}", response);

                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Not connected or authenticated");
                    }

                    await smtp.DisconnectAsync(true);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception sending email");
                }
            }

            return false;
        }
    }
}
