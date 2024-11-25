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
        GetSegmentedErrors(List<WatchDogLogEvent> recordsForScanDate, EmailOptions emailOptions)
        {
            //var consecutiveCountResults = GetConsecutiveDays(emailOptions.ScanDate);
            var watchDogEventSummary = GetYearStats(emailOptions.ScanDate, emailOptions.ScanDate.AddDays(-1));
            //var recordsWithPreviousDayOccurence = watchDogEventSummary.Where(w => w.OccurredOnSpecificDate == true).ToList();
            var (recordsForLast12Months, recordsForDayBeforeScanDate) = FetchRecords(emailOptions);

            var countAndDateLookupForLast12Months = CreateCountAndDateLookup(recordsForLast12Months);
            var countForDayBeforeScanDate = CreateDayBeforeCountLookup(recordsForDayBeforeScanDate);

            var allConvertedRecords = ConvertRecords(recordsForScanDate, countAndDateLookupForLast12Months);
            var orderedConvertedRecords = allConvertedRecords.OrderByDescending(record => record.ConsecutiveOccurenceCount).ToList();

            return CategorizeIssues(allConvertedRecords, countForDayBeforeScanDate);
        }

        public List<ConsecutiveDayResult> GetConsecutiveDays(DateTime scanDate)
        {
            DateTime previousDate = scanDate.AddDays(-1);

            var consecutiveDaysResult = watchDogLogEventRepository.GetList()
                .Where(e => e.Timestamp.Date == scanDate.Date || e.Timestamp.Date == previousDate.Date)
                .GroupBy(e => new
                {
                    e.LocationId,
                    e.ComponentType,
                    e.ComponentId,
                    e.Phase,
                    e.IssueType
                })
                .Where(group =>
                    group.Any(e => e.Timestamp.Date == previousDate.Date) &&
                    group.Any(e => e.Timestamp.Date == scanDate.Date)) // Ensure events exist for both dates
                .Select(group => new ConsecutiveDayResult
                {
                    LocationId = group.Key.LocationId,
                    ComponentType = group.Key.ComponentType,
                    ComponentId = group.Key.ComponentId,
                    Phase = group.Key.Phase,
                    IssueType = group.Key.IssueType,
                    ConsecutiveDays = 2 // Fixed to 2 since we're explicitly checking for two consecutive dates
                })
                .ToList();

            return consecutiveDaysResult;
        }
        
        public List<WatchDogEventSummary> GetYearStats(DateTime scanDate, DateTime previousDayDate)
        {
            DateTime previousDate = scanDate.AddDays(-1);

            var consecutiveDaysResult = watchDogLogEventRepository.GetList()
                .Where(e => e.Timestamp >= scanDate.AddMonths(-12) && e.Timestamp < scanDate)
                .GroupBy(e => new
                {
                    e.LocationId,
                    e.ComponentType,
                    e.ComponentId,
                    e.Phase,
                    e.IssueType
                })
                .Select(g => new WatchDogEventSummary
                {
                    LocationId = g.Key.LocationId,
                    ComponentType = g.Key.ComponentType,
                    ComponentId = g.Key.ComponentId,
                    Phase = g.Key.Phase,
                    IssueType = g.Key.IssueType,
                    OccurrenceCount = g.Count(),
                    OccurredOnSpecificDate = g.Any(e => e.Timestamp.Date == previousDayDate.Date) ? 1 : 0,
                    FirstOccurrenceDate = g.Min(e => e.Timestamp.Date)
                })
                .OrderByDescending(x => x.OccurrenceCount)
                .ToList();

            return consecutiveDaysResult;
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
    public class ConsecutiveDayResult
    {
        public int LocationId { get; set; }           // The ID of the location
        public WatchDogComponentTypes ComponentType { get; set; }    // The type of the component
        public int ComponentId { get; set; }         // The ID of the component
        public int? Phase { get; set; }            // The phase associated with the event
        public WatchDogIssueTypes IssueType { get; set; }        // The type of issue
        public int ConsecutiveDays { get; set; }     // The number of consecutive days up to the specific date

        public override string ToString()
        {
            return $"LocationId: {LocationId}, ComponentType: {ComponentType}, ComponentId: {ComponentId}, " +
                   $"Phase: {Phase}, IssueType: {IssueType}, ConsecutiveDays: {ConsecutiveDays}";
        }
    }

    public class WatchDogEventSummary
    {
        public int LocationId { get; set; }           // The ID of the location
        public WatchDogComponentTypes ComponentType { get; set; }    // The type of the component
        public int ComponentId { get; set; }         // The ID of the component
        public int? Phase { get; set; }            // The phase associated with the event
        public WatchDogIssueTypes IssueType { get; set; }        // The type of issue
        public int OccurrenceCount { get; set; }     // The total number of occurrences
        public int OccurredOnSpecificDate { get; set; } // Whether the event occurred on the specific date (1 for true, 0 for false)
        public DateTime FirstOccurrenceDate { get; set; } // The date of the first occurrence

        // Optional: ToString() override for debugging or logging purposes
        public override string ToString()
        {
            return $"LocationId: {LocationId}, ComponentType: {ComponentType}, ComponentId: {ComponentId}, " +
                   $"Phase: {Phase}, IssueType: {IssueType}, OccurrenceCount: {OccurrenceCount}, " +
                   $"OccurredOnSpecificDate: {OccurredOnSpecificDate}, FirstOccurrenceDate: {FirstOccurrenceDate:yyyy-MM-dd}";
        }
    }


}
