#region license
// Copyright 2024 Utah Departement of Transportation
// for Identity - Identity.Business.EmailSender/EmailService.cs
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
