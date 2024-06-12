#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - %Namespace%/SMTPMailService.cs
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
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace WatchDog.Models
{
    public class SMTPMailService : IMailService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<SMTPMailService> logger;
        SmtpClient smtp;
        public SMTPMailService(IConfiguration configuration, ILogger<SMTPMailService> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public void ConnectAndAuthenticate()
        {
            smtp = new SmtpClient();
            var host = configuration.GetValue<string>("SmtpSettings:Host");
            var port = configuration.GetValue<int>("SmtpSettings:Port");
            var userName = configuration.GetValue<string>("SmtpSettings:UserName");
            var password = configuration.GetValue<string>("SmtpSettings:Password");
            var enableSsl = configuration.GetValue<bool>("SmtpSettings:EnableSsl");
            try
            {
                logger.LogInformation($"Connecting to {host} on port {port} with SSL: {enableSsl}");
                smtp.Connect(host, port, enableSsl == true ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                logger.LogInformation($"Connected to {host} on port {port} with SSL: {enableSsl}");
                logger.LogInformation($"Authenticating with {userName}");
                smtp.Authenticate(userName, password);
                logger.LogInformation($"Authenticated with {userName}");
            }
            catch (System.Exception ex)
            {
                logger.LogError($"Unable to connect to email host: {ex.Message}");
            }
        }

        public async Task<bool> SendEmailAsync(string from, List<ApplicationUser> users, string subject, string body)
        {
            if (smtp is null)
            {
                throw new ArgumentNullException(nameof(smtp));
            }
            if (!smtp.IsConnected || !smtp.IsAuthenticated)
            {
                ConnectAndAuthenticate();
                if (!smtp.IsConnected || !smtp.IsAuthenticated)
                    return false;
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
