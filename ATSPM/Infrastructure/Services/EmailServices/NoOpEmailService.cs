#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.EmailServices/NoOpEmailService.cs
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

using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace Utah.Udot.Atspm.Infrastructure.Services.EmailServices
{
    /// <summary>
    /// Fallback email service used when no provider configuration exists.
    /// </summary>
    public class NoOpEmailService : ServiceObjectBase, IEmailService
    {
        private readonly ILogger<NoOpEmailService> _logger;

        public NoOpEmailService(ILogger<NoOpEmailService> logger) : base(true)
        {
            _logger = logger;
        }

        public Task<bool> SendEmailAsync(MailMessage message)
        {
            _logger.LogWarning(
                "Email send skipped because no email provider is configured. Subject: {Subject}, To: {Recipients}",
                message.Subject,
                string.Join(", ", message.To.Select(t => t.Address)));

            return Task.FromResult(false);
        }
    }
}
