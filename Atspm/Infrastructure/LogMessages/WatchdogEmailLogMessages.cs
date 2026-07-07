#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/WatchdogEmailLogMessages.cs
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

using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Log messages for the watchdog email service.
    /// </summary>
    public partial class WatchdogEmailLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for the watchdog email service.
        /// </summary>
        /// <param name="logger">The logger instance used to emit structured log messages.</param>
        /// <param name="serviceName">Name of the consuming service, added as a contextual label.</param>
        public WatchdogEmailLogMessages(ILogger logger, string serviceName)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "service", serviceName }
            });
        }

        /// <summary>
        /// Logs the resolved watchdog email recipient counts.
        /// </summary>
        /// <param name="totalUsers">Total number of watchdog users.</param>
        /// <param name="adminSubscriberUsers">Number of users who can receive the all-locations email.</param>
        /// <param name="regions">Number of regions.</param>
        /// <param name="jurisdictions">Number of jurisdictions.</param>
        /// <param name="areas">Number of areas.</param>
        [LoggerMessage(EventId = 1240, EventName = "Email Recipients Resolved", Level = LogLevel.Information, Message = "Watchdog email recipients resolved. Total watchdog users: {totalUsers}, admin subscriber users: {adminSubscriberUsers}, regions: {regions}, jurisdictions: {jurisdictions}, areas: {areas}")]
        public partial void EmailRecipientsResolved(int totalUsers, int adminSubscriberUsers, int regions, int jurisdictions, int areas);

        /// <summary>
        /// Logs that a null parameter was passed to the message builder.
        /// </summary>
        [LoggerMessage(EventId = 1241, EventName = "Null Parameter In GetMessage", Level = LogLevel.Error, Message = "Null parameter passed to GetMessage")]
        public partial void NullParameterInGetMessage();

        /// <summary>
        /// Logs that a watchdog email was sent successfully.
        /// </summary>
        /// <param name="scope">The email scope (e.g. Region, Jurisdiction, Area, All Locations).</param>
        /// <param name="name">The name of the scoped entity.</param>
        /// <param name="recipientCount">The number of recipients the email was sent to.</param>
        [LoggerMessage(EventId = 1242, EventName = "Email Sent", Level = LogLevel.Information, Message = "Watchdog email sent - {scope}: {name} to {recipientCount} recipient(s).")]
        public partial void EmailSent(string scope, string name, int recipientCount);

        /// <summary>
        /// Logs that a watchdog email failed to send.
        /// </summary>
        /// <param name="scope">The email scope (e.g. Region, Jurisdiction, Area, All Locations).</param>
        /// <param name="name">The name of the scoped entity.</param>
        /// <param name="ex">The exception thrown while sending the email.</param>
        [LoggerMessage(EventId = 1243, EventName = "Email Send Failed", Level = LogLevel.Error, Message = "Failed to send watchdog email - {scope}: {name}")]
        public partial void EmailSendFailed(string scope, string name, Exception ex);

        /// <summary>
        /// Logs that the configured email service returned a failed send result.
        /// </summary>
        /// <param name="scope">The email scope (e.g. Region, Jurisdiction, Area, All Locations).</param>
        /// <param name="name">The name of the scoped entity.</param>
        [LoggerMessage(EventId = 1244, EventName = "Email Send Returned False", Level = LogLevel.Warning, Message = "Watchdog email was not sent - {scope}: {name}")]
        public partial void EmailSendReturnedFalse(string scope, string name);
    }
}
