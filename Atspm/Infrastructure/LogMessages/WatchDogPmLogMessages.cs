#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/WatchDogPmLogMessages.cs
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
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Log messages for the watchdog PM (record count, detector, approach) analysis service.
    /// </summary>
    public partial class WatchDogPmLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for the watchdog PM analysis service.
        /// </summary>
        /// <param name="logger">The logger instance used to emit structured log messages.</param>
        /// <param name="serviceName">Name of the consuming service, added as a contextual label.</param>
        public WatchDogPmLogMessages(ILogger logger, string serviceName)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "service", serviceName }
            });
        }

        /// <summary>
        /// Logs an exception encountered while checking ramp metering detections.
        /// </summary>
        /// <param name="issueType">The issue type being checked.</param>
        /// <param name="locationId">The location identifier of the location being checked.</param>
        /// <param name="ex">The exception thrown while checking detections.</param>
        [LoggerMessage(EventId = 1220, EventName = "Check Detections Error", Level = LogLevel.Error, Message = "{issueType} {locationId}")]
        public partial void CheckDetectionsError(WatchDogIssueTypes issueType, int locationId, Exception ex);

        /// <summary>
        /// Logs an exception encountered while checking a detector for low hits.
        /// </summary>
        /// <param name="detectorId">The detector identifier being checked.</param>
        /// <param name="ex">The exception thrown while checking for low detector hits.</param>
        [LoggerMessage(EventId = 1221, EventName = "Low Detector Hits Error", Level = LogLevel.Error, Message = "CheckForLowDetectorHits {detectorId}")]
        public partial void LowDetectorHitsError(int detectorId, Exception ex);

        /// <summary>
        /// Logs that a location has a sufficient number of records.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier.</param>
        /// <param name="recordCount">The number of records found.</param>
        [LoggerMessage(EventId = 1222, EventName = "Record Count Sufficient", Level = LogLevel.Debug, Message = "Location {locationIdentifier} has {recordCount} records")]
        public partial void RecordCountSufficient(string locationIdentifier, int recordCount);

        /// <summary>
        /// Logs that a location does not have a sufficient number of records.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier.</param>
        [LoggerMessage(EventId = 1223, EventName = "Record Count Insufficient", Level = LogLevel.Debug, Message = "Location {locationIdentifier} Does Not Have Sufficient records")]
        public partial void RecordCountInsufficient(string locationIdentifier);

        /// <summary>
        /// Logs that a phase in use has no corresponding configured approach.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier.</param>
        /// <param name="phaseNumber">The phase number that is not configured.</param>
        [LoggerMessage(EventId = 1224, EventName = "Unconfigured Approach", Level = LogLevel.Debug, Message = "Location {locationIdentifier} {phaseNumber} Not Configured")]
        public partial void UnconfiguredApproach(string locationIdentifier, int phaseNumber);

        /// <summary>
        /// Logs that the PM analysis has started.
        /// </summary>
        /// <param name="locationCount">The number of locations to analyze.</param>
        [LoggerMessage(EventId = 1225, EventName = "PM Analysis Started", Level = LogLevel.Information, Message = "PM analysis started for {locationCount} location(s).")]
        public partial void AnalysisStarted(int locationCount);

        /// <summary>
        /// Logs that the PM analysis has completed.
        /// </summary>
        /// <param name="issueCount">The number of issues produced.</param>
        [LoggerMessage(EventId = 1226, EventName = "PM Analysis Completed", Level = LogLevel.Information, Message = "PM analysis completed with {issueCount} issue(s).")]
        public partial void AnalysisCompleted(int issueCount);

        /// <summary>
        /// Logs that the PM analysis was cancelled before completing.
        /// </summary>
        /// <param name="issueCount">The number of issues produced before cancellation.</param>
        [LoggerMessage(EventId = 1227, EventName = "PM Analysis Cancelled", Level = LogLevel.Information, Message = "PM analysis cancelled after {issueCount} issue(s).")]
        public partial void AnalysisCancelled(int issueCount);

        /// <summary>
        /// Logs that a location was skipped because its data query failed (e.g. a SQL timeout).
        /// </summary>
        /// <param name="locationIdentifier">The location identifier that was skipped.</param>
        /// <param name="ex">The exception that caused the location to be skipped.</param>
        [LoggerMessage(EventId = 1251, EventName = "PM Location Skipped", Level = LogLevel.Warning, Message = "Skipping location {locationIdentifier} in PM analysis due to a data query failure.")]
        public partial void LocationScanFailed(string locationIdentifier, Exception ex);
    }
}
