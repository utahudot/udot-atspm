#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/WatchDogRampLogMessages.cs
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
    /// Log messages for the watchdog ramp detector analysis service.
    /// </summary>
    public partial class WatchDogRampLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for the watchdog ramp detector analysis service.
        /// </summary>
        /// <param name="logger">The logger instance used to emit structured log messages.</param>
        /// <param name="serviceName">Name of the consuming service, added as a contextual label.</param>
        public WatchDogRampLogMessages(ILogger logger, string serviceName)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "service", serviceName }
            });
        }

        /// <summary>
        /// Logs an exception encountered while checking a ramp detector for low hits.
        /// </summary>
        /// <param name="detectorId">The detector identifier being checked.</param>
        /// <param name="ex">The exception thrown while checking for low ramp detector hits.</param>
        [LoggerMessage(EventId = 1230, EventName = "Low Ramp Detector Hits Error", Level = LogLevel.Error, Message = "CheckForLowRampDetectorHits {detectorId}")]
        public partial void LowRampDetectorHitsError(int detectorId, Exception ex);

        /// <summary>
        /// Logs that the ramp analysis has started.
        /// </summary>
        /// <param name="locationCount">The number of locations to analyze.</param>
        [LoggerMessage(EventId = 1231, EventName = "Ramp Analysis Started", Level = LogLevel.Information, Message = "Ramp analysis started for {locationCount} location(s).")]
        public partial void AnalysisStarted(int locationCount);

        /// <summary>
        /// Logs that the ramp analysis has completed.
        /// </summary>
        /// <param name="issueCount">The number of issues produced.</param>
        [LoggerMessage(EventId = 1232, EventName = "Ramp Analysis Completed", Level = LogLevel.Information, Message = "Ramp analysis completed with {issueCount} issue(s).")]
        public partial void AnalysisCompleted(int issueCount);

        /// <summary>
        /// Logs that the ramp analysis was cancelled before completing.
        /// </summary>
        /// <param name="issueCount">The number of issues produced before cancellation.</param>
        [LoggerMessage(EventId = 1233, EventName = "Ramp Analysis Cancelled", Level = LogLevel.Information, Message = "Ramp analysis cancelled after {issueCount} issue(s).")]
        public partial void AnalysisCancelled(int issueCount);

        /// <summary>
        /// Logs that a location was skipped because its data query failed (e.g. a SQL timeout).
        /// </summary>
        /// <param name="locationIdentifier">The location identifier that was skipped.</param>
        /// <param name="ex">The exception that caused the location to be skipped.</param>
        [LoggerMessage(EventId = 1252, EventName = "Ramp Location Skipped", Level = LogLevel.Warning, Message = "Skipping location {locationIdentifier} in ramp analysis due to a data query failure.")]
        public partial void LocationScanFailed(string locationIdentifier, Exception ex);
    }
}
