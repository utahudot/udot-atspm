﻿using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Configuration;
using ATSPM.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.EmailServices
{
    public class SendGridEmailService : ServiceObjectBase, IEmailService
    {
        private readonly EmailConfiguration _options;
        private readonly ILogger _logger;

        public SendGridEmailService(IOptionsSnapshot<EmailConfiguration> options, ILogger<SendGridEmailService> logger) : base(true)
        {
            _options = options?.Get(this.GetType().Name) ?? options?.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> SendEmailAsync(MailMessage message)
        {
            _logger.LogDebug("SendEmail message: {message}", message);

            if (!string.IsNullOrEmpty(_options.Password))
            {
                var client = new SendGridClient(_options.Password);

                var from = new EmailAddress(message.From.Address, message.From.DisplayName);
                var subject = message.Subject;
                var to = message.To.Select(s => new EmailAddress(s.Address, s.DisplayName)).ToList();
                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, to, message.Subject, message.IsBodyHtml ? null : message.Body, message.IsBodyHtml ? message.Body : null);

                try
                {
                    _logger.LogDebug("SendEmail sending: {msg}", msg);

                    var response = await client.SendEmailAsync(msg);

                    _logger.LogInformation("SendEmail response: {response}", response);

                    return response.IsSuccessStatusCode;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception sending email");
                }
            }
            else
            {
                _logger.LogWarning("Key is null or empty");
            }

            return false;
        }
    }
}