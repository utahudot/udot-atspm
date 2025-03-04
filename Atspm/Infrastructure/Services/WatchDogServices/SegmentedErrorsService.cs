#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices/SegmentedErrorsService.cs
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

using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Repositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    public class SegmentedErrorsService
    {
        private readonly IWatchDogEventLogRepository watchDogLogEventRepository;

        public SegmentedErrorsService(IWatchDogEventLogRepository watchDogLogEventRepository)
        {
            this.watchDogLogEventRepository = watchDogLogEventRepository;
        }

        public (List<WatchDogLogEventWithCountAndDate> newIssues, List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues, List<WatchDogLogEventWithCountAndDate> recurringIssues)
        GetSegmentedErrors(List<WatchDogLogEvent> recordsForScanDate, WatchdogEmailOptions WatchdogEmailOptions)
        {
            var (recordsForLast12Months, recordsForDayBeforeScanDate) = FetchRecords(WatchdogEmailOptions);
            var countAndDateLookupForLast12Months = CreateCountAndDateLookup(recordsForLast12Months, WatchdogEmailOptions.ScanDate.AddDays(-1));
            var allConvertedRecords = ConvertRecords(recordsForScanDate, countAndDateLookupForLast12Months);

            return CategorizeIssues(allConvertedRecords, WatchdogEmailOptions.Sort);
        }

        private (List<WatchDogLogEvent> recordsForLast12Months, List<WatchDogLogEvent> recordsForDayBeforeScanDate)
        FetchRecords(WatchdogEmailOptions WatchdogEmailOptions)
        {
            if (WatchdogEmailOptions.WeekdayOnly && WatchdogEmailOptions.ScanDate.DayOfWeek == DayOfWeek.Monday)
            {
                var recordsForDayBeforeScanDate = watchDogLogEventRepository.GetList(w => w.Timestamp >= WatchdogEmailOptions.ScanDate.AddDays(-3) &&
                                    w.Timestamp < WatchdogEmailOptions.ScanDate.AddDays(-2)).ToList();
                var recordsForLast12Months = watchDogLogEventRepository.GetList(w => w.Timestamp >= WatchdogEmailOptions.ScanDate.AddDays(-3).AddMonths(-12) &&
                    w.Timestamp < WatchdogEmailOptions.ScanDate.AddDays(-2)).ToList();
                return (recordsForLast12Months, recordsForDayBeforeScanDate);
            }
            else
            {
                var recordsForDayBeforeScanDate = watchDogLogEventRepository.GetList(w => w.Timestamp >= WatchdogEmailOptions.ScanDate.AddDays(-1) &&
                                   w.Timestamp < WatchdogEmailOptions.ScanDate).ToList();
                var recordsForLast12Months = watchDogLogEventRepository.GetList(w => w.Timestamp >= WatchdogEmailOptions.ScanDate.AddDays(-1).AddMonths(-12) &&
                    w.Timestamp < WatchdogEmailOptions.ScanDate).ToList();
                return (recordsForLast12Months, recordsForDayBeforeScanDate);
            }
        }

        public static Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase), (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)>
        CreateCountAndDateLookup(List<WatchDogLogEvent> recordsForLast12Months, DateTime previousDate)
        {
            return recordsForLast12Months
                .GroupBy(r => (r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase))
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var orderedEvents = group.OrderBy(e => e.Timestamp).ToList();
                        var firstOccurrence = orderedEvents.First().Timestamp;
                        var consecutiveCount = CalculateConsecutiveOccurrences(orderedEvents, previousDate);
                        return (
                            Count: group.Count(),
                            DateOfFirstOccurrence: group.Min(e => e.Timestamp),
                            ConsecutiveOccurrenceCount: consecutiveCount
                        );
                    }
                );
        }

        public static int CalculateConsecutiveOccurrences(List<WatchDogLogEvent> orderedEvents, DateTime previousDay)
        {
            orderedEvents = orderedEvents.OrderByDescending(e => e.Timestamp).ToList();
            int streak = 0;

            // Ensure the list is not empty
            if (orderedEvents.Count == 0)
                return streak;

            // Check if the first event matches the `previousDay`
            if (orderedEvents[0].Timestamp.Date != previousDay.Date)
                return streak;

            // Start the streak at 1 because the first event matches
            streak = 1;

            // Iterate through the remaining events to calculate the streak
            for (int i = 1; i < orderedEvents.Count; i++)
            {
                // Check if the current event is consecutive to the previous one
                if ((orderedEvents[i - 1].Timestamp.Date - orderedEvents[i].Timestamp.Date).TotalDays == 1)
                {
                    streak++;
                }
                else
                {
                    break; // Streak ends if days are not consecutive
                }
            }

            return streak;
        }

        public static List<WatchDogLogEventWithCountAndDate>
        ConvertRecords(List<WatchDogLogEvent> recordsForScanDate, Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase), (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)> countAndDateLookupForLast12Months)
        {
            return recordsForScanDate
                .Select(r => new WatchDogLogEventWithCountAndDate(r.LocationId, r.LocationIdentifier, r.Timestamp, r.ComponentType, r.ComponentId, r.IssueType, r.Details, r.Phase)
                {
                    EventCount = countAndDateLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase), out var data) ? data.Count : 0,
                    DateOfFirstInstance = countAndDateLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase), out data) ? data.DateOfFirstOccurrence : r.Timestamp,
                    ConsecutiveOccurenceCount = countAndDateLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase), out data) ? data.ConsecutiveOccurrenceCount : 0
                })
                .ToList();
        }

        public static (List<WatchDogLogEventWithCountAndDate> newIssues,
               List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues,
               List<WatchDogLogEventWithCountAndDate> recurringIssues)
        CategorizeIssues(List<WatchDogLogEventWithCountAndDate> allConvertedRecords, string sortOption)
        {
            // Issues that have never occurred before
            var newIssues = allConvertedRecords
                .Where(r => r.EventCount == 0)
                .ToList();

            // Issues that occurred on the previous day
            var dailyRecurringIssues = allConvertedRecords
                .Where(r => r.EventCount != 0 &&
                            r.ConsecutiveOccurenceCount > 0)
                .ToList();

            // Issues that have occurred before and are not daily recurring
            var recurringIssues = allConvertedRecords
                .Where(r => r.EventCount != 0)
                .Except(newIssues.Concat(dailyRecurringIssues))
                .ToList();

            // Apply sorting based on the sort option
            Func<WatchDogLogEventWithCountAndDate, object> sortKeySelector = sortOption.ToLower() switch
            {
                "error" => r => r.EventCount, // Sort by IssueType (Error)
                "consecutive" => r => r.ConsecutiveOccurenceCount, // Sort by ConsecutiveOccurenceCount
                "location" => r => r.LocationIdentifier, // Sort by LocationId
                _ => r => r.Timestamp // Default sort by Timestamp
            };

            newIssues = newIssues.OrderBy(sortKeySelector).ThenBy(i => i.IssueType).ToList();
            dailyRecurringIssues = dailyRecurringIssues.OrderBy(sortKeySelector).ThenBy(i => i.IssueType).ToList();
            recurringIssues = recurringIssues.OrderBy(sortKeySelector).ThenBy(i => i.IssueType).ToList();

            return (newIssues, dailyRecurringIssues, recurringIssues);
        }



    }
}
