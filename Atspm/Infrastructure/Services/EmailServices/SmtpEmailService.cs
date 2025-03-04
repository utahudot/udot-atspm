#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.EmailServices/SmtpEmailService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;

namespace Utah.Udot.Atspm.Infrastructure.Services.EmailServices
{
    /// <summary>
    /// Smtp email service
    /// </summary>
    public class SmtpEmailService : ServiceObjectBase, IEmailService
    {
        private readonly EmailConfiguration _options;
        private readonly ILogger _logger;

        /// <summary>
        /// Smtp email service
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public SmtpEmailService(IOptionsSnapshot<EmailConfiguration> options, ILogger<SmtpEmailService> logger) : base(true)
        {
            _options = options?.Get(GetType().Name) ?? options?.Value;
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
                    if (!String.IsNullOrEmpty(_options.UserName) || !String.IsNullOrEmpty(_options.Password))
                    {
                        await smtp.AuthenticateAsync(_options.UserName, _options.Password);
                    }

                    if (smtp.IsConnected)
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
