#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/ReportsLoggerLogMessages.cs
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
    /// Log messages for <see cref="IReportService{Tin, Tout}"/> implementations
    /// </summary>
    public partial class ReportsLoggerLogMessages<Tin, Tout>
    {
        private readonly ILogger _logger;

        //TODO: This logger has been placed in the controllers for the ReportApi when it should be injected into the IReportService directoy and log there, Controllers have their own logging mechanism

        /// <summary>
        /// Log messages for <see cref="IReportService{Tin, Tout}"/> implementations
        /// </summary>
        /// <param name="logger">Abstract logging providers</param>
        /// <param name="reportService">Reporting service to log messages for</param>
        public ReportsLoggerLogMessages(ILogger logger, IReportService<Tin, Tout> reportService)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "ReportService", reportService.GetType().Name }
            });
        }

        /// <summary>
        /// Report has started
        /// </summary>
        /// <param name="time"></param>
        /// <param name="name"></param>
        [LoggerMessage(EventId = 1101, EventName = "Report has started", Level = LogLevel.Information, Message = "Report has started at {time}, name: {name}")]
        public partial void ReportStartedMessage(DateTime time, string name);

        /// <summary>
        /// Report has completed
        /// </summary>
        /// <param name="time"></param>
        /// <param name="name"></param>
        [LoggerMessage(EventId = 1102, EventName = "Report has completed", Level = LogLevel.Information, Message = "Report has completed at {time}, name: {name}")]
        public partial void ReportCompletedMessage(DateTime time, string name);

        /// <summary>
        /// Report exception
        /// </summary>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1103, EventName = "Report execution exception", Level = LogLevel.Error, Message = "Exception executing Report")]
        public partial void ReportExecutionException(Exception ex = null);
    }
}
