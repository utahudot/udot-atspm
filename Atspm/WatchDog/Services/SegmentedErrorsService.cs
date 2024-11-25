#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - Utah.Udot.ATSPM.WatchDog.Services/SegmentedErrorsService.cs
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
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.WatchDog.Models;

namespace Utah.Udot.ATSPM.WatchDog.Services
{
    public class SegmentedErrorsService
    {
        private readonly IWatchDogEventLogRepository watchDogLogEventRepository;

        public SegmentedErrorsService(IWatchDogEventLogRepository watchDogLogEventRepository)
        {
            this.watchDogLogEventRepository = watchDogLogEventRepository;
        }

        public (List<WatchDogLogEventWithCountAndDate> newIssues, List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues, List<WatchDogLogEventWithCountAndDate> recurringIssues)
        GetSegmentedErrors(List<WatchDogLogEvent> recordsForScanDate, EmailOptions emailOptions)
        {
            var (recordsForLast12Months, recordsForDayBeforeScanDate) = FetchRecords(emailOptions);

            var countAndDateLookupForLast12Months = CreateCountAndDateLookup(recordsForLast12Months);
            var countForDayBeforeScanDate = CreateDayBeforeCountLookup(recordsForDayBeforeScanDate);

            var allConvertedRecords = ConvertRecords(recordsForScanDate, countAndDateLookupForLast12Months);
            var orderedConvertedRecords = allConvertedRecords.OrderByDescending(record => record.ConsecutiveOccurenceCount).ToList();

            return CategorizeIssues(allConvertedRecords, countForDayBeforeScanDate);
        }

        private (List<WatchDogLogEvent> recordsForLast12Months, List<WatchDogLogEvent> recordsForDayBeforeScanDate)
        FetchRecords(EmailOptions emailOptions)
        {
            if (emailOptions.WeekdayOnly && emailOptions.ScanDate.DayOfWeek == DayOfWeek.Monday)
            {
                var recordsForDayBeforeScanDate = watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-3) &&
                                    w.Timestamp < emailOptions.ScanDate.AddDays(-2)).ToList();
                var recordsForLast12Months = watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-3).AddMonths(-12) &&
                    w.Timestamp < emailOptions.ScanDate.AddDays(-2)).ToList();
                return (recordsForLast12Months, recordsForDayBeforeScanDate);
            }
            else
            {
                var recordsForDayBeforeScanDate = watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-1) &&
                                   w.Timestamp < emailOptions.ScanDate).ToList();
                var recordsForLast12Months = watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-1).AddMonths(-12) &&
                    w.Timestamp < emailOptions.ScanDate).ToList();
                return (recordsForLast12Months, recordsForDayBeforeScanDate);
            }
        }

        private static Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase), (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)>
        CreateCountAndDateLookup(List<WatchDogLogEvent> recordsForLast12Months)
        {
            return recordsForLast12Months
                .GroupBy(r => (r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase))
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var orderedEvents = group.OrderBy(e => e.Timestamp).ToList();
                        var firstOccurrence = orderedEvents.First().Timestamp;
                        var consecutiveCount = CalculateConsecutiveOccurrences(orderedEvents);
                        return (
                            Count: group.Count(),
                            DateOfFirstOccurrence: group.Min(e => e.Timestamp),
                            ConsecutiveOccurrenceCount: consecutiveCount
                        );
                    }
                );
        }

        private static int CalculateConsecutiveOccurrences(List<WatchDogLogEvent> orderedEvents)
        {
            int maxConsecutiveDays = 1; // Keep track of the longest streak
            int currentStreak = 1;

            for (int i = 1; i < orderedEvents.Count; i++)
            {
                // Check if the event occurred on the next consecutive day
                if ((orderedEvents[i].Timestamp - orderedEvents[i - 1].Timestamp).TotalDays == 1)
                {
                    currentStreak++;
                    maxConsecutiveDays = Math.Max(maxConsecutiveDays, currentStreak);
                }
                else
                {
                    // Reset streak if there is a gap in consecutive days
                    currentStreak = 1;
                }
            }

            return maxConsecutiveDays;
        }

        private static Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase), int>
        CreateDayBeforeCountLookup(List<WatchDogLogEvent> recordsForDayBeforeScanDate)
        {
            return recordsForDayBeforeScanDate
                .GroupBy(r => (r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase))
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );
        }

        private static List<WatchDogLogEventWithCountAndDate>
        ConvertRecords(List<WatchDogLogEvent> recordsForScanDate, Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase), (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)> countAndDateLookupForLast12Months)
        {
            return recordsForScanDate
                .Select(r => new WatchDogLogEventWithCountAndDate(r.LocationId, r.LocationIdentifier, r.Timestamp, r.ComponentType, r.ComponentId, r.IssueType, r.Details, r.Phase)
                {
                    EventCount = countAndDateLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase), out var data) ? data.Count : 0,
                    DateOfFirstInstance = countAndDateLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase), out data) ? data.DateOfFirstOccurrence : r.Timestamp,
                    ConsecutiveOccurenceCount = countAndDateLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase), out data) ? data.ConsecutiveOccurrenceCount : 1
                })
                .ToList();
        }

        private static (List<WatchDogLogEventWithCountAndDate> newIssues, List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues, List<WatchDogLogEventWithCountAndDate> recurringIssues)
        CategorizeIssues(List<WatchDogLogEventWithCountAndDate> allConvertedRecords, Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase), int> countForDayBeforeScanDate)
        {
            var newIssues = allConvertedRecords
                .Where(r => r.EventCount == 0)
                .ToList();

            var dailyRecurringIssues = allConvertedRecords
                .Where(r => countForDayBeforeScanDate.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase), out var dayBeforeCount) && dayBeforeCount == 1)
                .ToList();

            var recurringIssues = allConvertedRecords
                .Except(newIssues.Concat(dailyRecurringIssues))
                .ToList();

            return (newIssues, dailyRecurringIssues, recurringIssues);
        }
    }
}
