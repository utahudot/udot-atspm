using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Configuration;
using ATSPM.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.EmailServices
{
    public class SendGridEmailService : ServiceObjectBase, IEmailService
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly ILogger _logger;

        public SendGridEmailService(IOptions<EmailConfiguration> options, ILogger<SendGridEmailService> logger) : base(true)
        {
            _emailConfiguration = options.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> SendEmailAsync(MailMessage message)
        {
            if (!string.IsNullOrEmpty(_emailConfiguration.Key))
            {
                var client = new SendGridClient(_emailConfiguration.Key);

                var from = new EmailAddress(message.From.Address, message.From.User);
                var subject = message.Subject;
                var to = message.To.Select(s => new EmailAddress(s.Address, s.User)).ToList();
                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, to, message.Subject, message.IsBodyHtml ? null : message.Body, message.IsBodyHtml ? message.Body : null);
                var response = await client.SendEmailAsync(msg);

                _logger.LogDebug("SendEmail Response: {response}", response);

                return response.IsSuccessStatusCode;
            }
            else
            {
                _logger.LogError("Key is empty");

                return false;
            }
        }
    }
}
