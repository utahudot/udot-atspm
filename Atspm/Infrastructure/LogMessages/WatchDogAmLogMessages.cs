#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/WatchDogAmLogMessages.cs
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
    /// Log messages for the watchdog AM (force off, max out, stuck ped) analysis service.
    /// </summary>
    public partial class WatchDogAmLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for the watchdog AM analysis service.
        /// </summary>
        /// <param name="logger">The logger instance used to emit structured log messages.</param>
        /// <param name="serviceName">Name of the consuming service, added as a contextual label.</param>
        public WatchDogAmLogMessages(ILogger logger, string serviceName)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "service", serviceName }
            });
        }

        /// <summary>
        /// Logs that the analysis phase collection could not be obtained for a location.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier being analyzed.</param>
        /// <param name="ex">The exception thrown while retrieving the analysis phase collection.</param>
        [LoggerMessage(EventId = 1210, EventName = "Analysis Phase Unavailable", Level = LogLevel.Error, Message = "Unable to get analysis phase for Location {locationIdentifier}")]
        public partial void AnalysisPhaseUnavailable(string locationIdentifier, Exception ex);

        /// <summary>
        /// Logs an exception encountered while checking a phase for max out errors.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier being analyzed.</param>
        /// <param name="phaseNumber">The phase number being analyzed.</param>
        /// <param name="ex">The exception thrown while checking for max out.</param>
        [LoggerMessage(EventId = 1211, EventName = "Max Out Check Error", Level = LogLevel.Error, Message = "{locationIdentifier} {phaseNumber} - Max Out Error")]
        public partial void MaxOutCheckError(string locationIdentifier, int phaseNumber, Exception ex);

        /// <summary>
        /// Logs an exception encountered while checking a phase for force off errors.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier being analyzed.</param>
        /// <param name="phaseNumber">The phase number being analyzed.</param>
        /// <param name="ex">The exception thrown while checking for force off.</param>
        [LoggerMessage(EventId = 1212, EventName = "Force Off Check Error", Level = LogLevel.Error, Message = "{locationIdentifier} {phaseNumber} - Force Off Error")]
        public partial void ForceOffCheckError(string locationIdentifier, int phaseNumber, Exception ex);

        /// <summary>
        /// Logs an exception encountered while checking a phase for stuck ped errors.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier being analyzed.</param>
        /// <param name="phaseNumber">The phase number being analyzed.</param>
        /// <param name="ex">The exception thrown while checking for stuck ped.</param>
        [LoggerMessage(EventId = 1213, EventName = "Stuck Ped Check Error", Level = LogLevel.Error, Message = "{locationIdentifier} {phaseNumber} - Stuck Ped Error")]
        public partial void StuckPedCheckError(string locationIdentifier, int phaseNumber, Exception ex);

        /// <summary>
        /// Logs a detected stuck ped (high pedestrian activation) condition.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier.</param>
        /// <param name="pedestrianActivations">The number of pedestrian activations detected.</param>
        [LoggerMessage(EventId = 1214, EventName = "Stuck Ped Detected", Level = LogLevel.Debug, Message = "Location {locationIdentifier} {pedestrianActivations} Pedestrian Activations")]
        public partial void StuckPedDetected(string locationIdentifier, int pedestrianActivations);

        /// <summary>
        /// Logs a detected force off threshold condition.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier.</param>
        [LoggerMessage(EventId = 1215, EventName = "Force Off Detected", Level = LogLevel.Debug, Message = "Location {locationIdentifier} Has ForceOff Errors")]
        public partial void ForceOffDetected(string locationIdentifier);

        /// <summary>
        /// Logs a detected max out threshold condition.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier.</param>
        [LoggerMessage(EventId = 1216, EventName = "Max Out Detected", Level = LogLevel.Debug, Message = "Location {locationIdentifier} Has MaxOut Errors")]
        public partial void MaxOutDetected(string locationIdentifier);

        /// <summary>
        /// Logs that the AM analysis has started.
        /// </summary>
        /// <param name="locationCount">The number of locations to analyze.</param>
        [LoggerMessage(EventId = 1217, EventName = "AM Analysis Started", Level = LogLevel.Information, Message = "AM analysis started for {locationCount} location(s).")]
        public partial void AnalysisStarted(int locationCount);

        /// <summary>
        /// Logs that the AM analysis has completed.
        /// </summary>
        /// <param name="issueCount">The number of issues produced.</param>
        [LoggerMessage(EventId = 1218, EventName = "AM Analysis Completed", Level = LogLevel.Information, Message = "AM analysis completed with {issueCount} issue(s).")]
        public partial void AnalysisCompleted(int issueCount);

        /// <summary>
        /// Logs that the AM analysis was cancelled before completing.
        /// </summary>
        /// <param name="issueCount">The number of issues produced before cancellation.</param>
        [LoggerMessage(EventId = 1219, EventName = "AM Analysis Cancelled", Level = LogLevel.Information, Message = "AM analysis cancelled after {issueCount} issue(s).")]
        public partial void AnalysisCancelled(int issueCount);

        /// <summary>
        /// Logs that a location was skipped because its data query failed (e.g. a SQL timeout).
        /// </summary>
        /// <param name="locationIdentifier">The location identifier that was skipped.</param>
        /// <param name="ex">The exception that caused the location to be skipped.</param>
        [LoggerMessage(EventId = 1250, EventName = "AM Location Skipped", Level = LogLevel.Warning, Message = "Skipping location {locationIdentifier} in AM analysis due to a data query failure.")]
        public partial void LocationScanFailed(string locationIdentifier, Exception ex);
    }
}
