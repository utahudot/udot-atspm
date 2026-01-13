#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/HostedServiceLogMessages.cs
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
    /// Provides strongly-typed logging methods for API usage events.
    /// Wraps an <see cref="ILogger"/> instance with contextual labels
    /// derived from a <see cref="UsageEntry"/>.
    /// </summary>
    public partial class ApiUsageLogMessage
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiUsageLogMessage"/> class.
        /// Adds contextual labels (such as TraceId, UserId, Route, Method, etc.)
        /// to the provided <see cref="ILogger"/> based on the supplied <see cref="UsageEntry"/>.
        /// </summary>
        /// <param name="logger">The logger instance used to emit structured log messages.</param>
        /// <param name="dataDownloadLog">The usage entry containing metadata about the API call.</param>
        public ApiUsageLogMessage(ILogger logger, UsageEntry dataDownloadLog)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string?>
        {
            { nameof(UsageEntry.TraceId), dataDownloadLog.TraceId },
            { nameof(UsageEntry.UserId), dataDownloadLog.UserId },
            { nameof(UsageEntry.Route), dataDownloadLog.Route },
            { nameof(UsageEntry.Method), dataDownloadLog.Method },
            { nameof(UsageEntry.StatusCode), dataDownloadLog.StatusCode.ToString() },
            { nameof(UsageEntry.DurationMs), dataDownloadLog.DurationMs.ToString() },
            { nameof(UsageEntry.Controller), dataDownloadLog.Controller },
            { nameof(UsageEntry.Action), dataDownloadLog.Action },
            { nameof(UsageEntry.RemoteIp), dataDownloadLog.RemoteIp },
            { nameof(UsageEntry.UserAgent), dataDownloadLog.UserAgent }
        });
        }

        /// <summary>
        /// Logs a successful API call at <see cref="LogLevel.Information"/>.
        /// </summary>
        /// <param name="usageEntry">The usage entry containing details of the API call.</param>
        [LoggerMessage(EventId = 201, EventName = "Api Call Successful", Level = LogLevel.Information, Message = "Api Call Successful: {usageEntry}")]
        public partial void CallSuccessful(UsageEntry usageEntry);

        /// <summary>
        /// Logs an API call that completed with a warning at <see cref="LogLevel.Warning"/>.
        /// Typically used when the status code indicates a non-critical issue.
        /// </summary>
        /// <param name="usageEntry">The usage entry containing details of the API call.</param>
        /// <param name="statusCode">The HTTP status code returned by the API call.</param>
        [LoggerMessage(EventId = 202, EventName = "Api Call Warning", Level = LogLevel.Warning, Message = "Api Call Has Warning: StatusCode:{statusCode} - {usageEntry}")]
        public partial void CallWarning(UsageEntry usageEntry, int statusCode);

        /// <summary>
        /// Logs an API call that failed with an error at <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="usageEntry">The usage entry containing details of the API call.</param>
        /// <param name="ex">The exception that occurred during the API call, if available.</param>
        [LoggerMessage(EventId = 203, EventName = "Api Call Error", Level = LogLevel.Error, Message = "Api Call Has Error: {usageEntry}")]
        public partial void CallError(UsageEntry usageEntry, Exception ex = null);
    }

}
