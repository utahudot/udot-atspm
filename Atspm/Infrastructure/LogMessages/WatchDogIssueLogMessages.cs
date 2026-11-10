using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Records the evidence for each issue actually added by a Watchdog scan.
    /// </summary>
    internal static partial class WatchDogIssueLogMessages
    {
        internal static void AddIssue(ILogger logger, string scanType, ConcurrentBag<WatchDogLogEvent> errors, WatchDogLogEvent issue, string reason)
        {
            if (errors.Contains(issue))
                return;

            errors.Add(issue);
            IssueAdded(logger, scanType, issue.LocationIdentifier, issue.LocationId, issue.Timestamp,
                issue.IssueType, issue.ComponentType, issue.ComponentId, issue.Phase, issue.Key, issue.Details, reason);
        }

        [LoggerMessage(EventId = 1253, EventName = "Watchdog Issue Added", Level = LogLevel.Information,
            Message = "Watchdog {scanType} added {issueType} for location {locationIdentifier} (ID {locationId}), scan date {scanDate:O}, {componentType} {componentId}, phase {phase}, key {key}. Details: {details}. Reason: {reason}")]
        private static partial void IssueAdded(ILogger logger, string scanType, string locationIdentifier, int locationId,
            DateTime scanDate, WatchDogIssueTypes issueType, WatchDogComponentTypes componentType, int componentId,
            int? phase, string key, string details, string reason);
    }
}
