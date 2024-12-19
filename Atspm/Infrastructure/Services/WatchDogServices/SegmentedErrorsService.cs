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
        GetSegmentedErrors(List<WatchDogLogEvent> recordsForScanDate, WatchdogEmailOptions emailOptions)
        {
            //var consecutiveCountResults = GetConsecutiveDays(emailOptions.ScanDate);
            //var watchDogEventSummary = GetYearStats(emailOptions.ScanDate, emailOptions.ScanDate.AddDays(-1));
            //var recordsWithPreviousDayOccurence = watchDogEventSummary.Where(w => w.OccurredOnSpecificDate == true).ToList();
            var (recordsForLast12Months, recordsForDayBeforeScanDate) = FetchRecords(emailOptions);

            var countAndDateLookupForLast12Months = CreateCountAndDateLookup(recordsForLast12Months, emailOptions.ScanDate);
            var countForDayBeforeScanDate = CreateDayBeforeCountLookup(recordsForDayBeforeScanDate);

            var allConvertedRecords = ConvertRecords(recordsForScanDate, countAndDateLookupForLast12Months);
            var orderedConvertedRecords = allConvertedRecords.OrderByDescending(record => record.ConsecutiveOccurenceCount).ToList();

            return CategorizeIssues(allConvertedRecords, countForDayBeforeScanDate);
        }

        //public List<ConsecutiveDayResult> GetConsecutiveDays(DateTime scanDate)
        //{
        //    DateTime previousDate = scanDate.AddDays(-1);

        //    var consecutiveDaysResult = watchDogLogEventRepository.GetList()
        //        .Where(e => e.Timestamp.Date == scanDate.Date || e.Timestamp.Date == previousDate.Date)
        //        .GroupBy(e => new
        //        {
        //            e.LocationId,
        //            e.ComponentType,
        //            e.ComponentId,
        //            e.Phase,
        //            e.IssueType
        //        })
        //        .Where(group =>
        //            group.Any(e => e.Timestamp.Date == previousDate.Date) &&
        //            group.Any(e => e.Timestamp.Date == scanDate.Date)) // Ensure events exist for both dates
        //        .Select(group => new ConsecutiveDayResult
        //        {
        //            LocationId = group.Key.LocationId,
        //            ComponentType = group.Key.ComponentType,
        //            ComponentId = group.Key.ComponentId,
        //            Phase = group.Key.Phase,
        //            IssueType = group.Key.IssueType,
        //            ConsecutiveDays = 2 // Fixed to 2 since we're explicitly checking for two consecutive dates
        //        })
        //        .ToList();

        //    return consecutiveDaysResult;
        //}
        
        //public List<WatchDogEventSummary> GetYearStats(DateTime scanDate, DateTime previousDayDate)
        //{
        //    DateTime previousDate = scanDate.AddDays(-1);

        //    var test = watchDogLogEventRepository.GetList()
        //        .Where(e => e.Timestamp >= scanDate.AddMonths(-12) && e.Timestamp < scanDate)
        //        .GroupBy(e => new WatchDogGroup
        //        {
        //            LocationId = e.LocationId,
        //            ComponentType = e.ComponentType,
        //            ComponentId = e.ComponentId,
        //            Phase = e.Phase ?? -1,
        //            IssueType = e.IssueType
        //        })
        //        .ToList();

        //    var consecutiveDaysResult = watchDogLogEventRepository.GetList()
        //        .Where(e => e.Timestamp >= scanDate.AddMonths(-12) && e.Timestamp < scanDate)
        //        .GroupBy(e => new
        //        {
        //            e.LocationId,
        //            e.ComponentType,
        //            e.ComponentId,
        //            e.Phase,
        //            e.IssueType
        //        })
        //        .Select(g => new WatchDogEventSummary
        //        {
        //            LocationId = g.Key.LocationId,
        //            ComponentType = g.Key.ComponentType,
        //            ComponentId = g.Key.ComponentId,
        //            Phase = g.Key.Phase,
        //            IssueType = g.Key.IssueType,
        //            OccurrenceCount = g.Count(),
        //            OccurredOnSpecificDate = g.Any(e => e.Timestamp.Date == previousDayDate.Date) ? 1 : 0,
        //            FirstOccurrenceDate = g.Min(e => e.Timestamp.Date)
        //        })
        //        .OrderByDescending(x => x.OccurrenceCount)
        //        .ToList();

        //    return consecutiveDaysResult;
        //}


        private (List<WatchDogLogEvent> recordsForLast12Months, List<WatchDogLogEvent> recordsForDayBeforeScanDate)
        FetchRecords(WatchdogEmailOptions emailOptions)
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
                var test = recordsForLast12Months.OrderBy(r => r.LocationIdentifier).ThenBy(r => r.ComponentType).ThenBy(r => r.ComponentId).ThenBy(r => r.IssueType).ThenBy(r => r.Phase).ToList();
                return (recordsForLast12Months, recordsForDayBeforeScanDate);
            }
        }

        public static Dictionary<(string LocationIdentifier, WatchDogComponentTypes ComponentType, int ComponentId,  WatchDogIssueTypes IssueType,  int? Phase), (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)>
        CreateCountAndDateLookup(List<WatchDogLogEvent> recordsForLast12Months, DateTime scanDate)
        {
            return recordsForLast12Months
                .GroupBy(r => (r.LocationIdentifier, r.ComponentType, r.ComponentId, r.IssueType, r.Phase))
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var orderedEvents = group.OrderBy(e => e.Timestamp).ToList();
                        var firstOccurrence = orderedEvents.First().Timestamp;
                        var consecutiveCount = CalculateLastConsecutiveOccurrences(orderedEvents, scanDate);
                        return (
                            Count: group.Count(),
                            DateOfFirstOccurrence: group.Min(e => e.Timestamp),
                            ConsecutiveOccurrenceCount: consecutiveCount
                        );
                    }
                );
        }

        public static int CalculateLastConsecutiveOccurrences(List<WatchDogLogEvent> events, DateTime scanDate)
        {
            if (events == null || events.Count == 0 || !events.Select(e => e.Timestamp.Date).Contains(scanDate.AddDays(-1)))
                return 0;

            // Ensure events are ordered by timestamp in descending order
            var orderedEvents = events.OrderByDescending(e => e.Timestamp).ToList();

            int currentStreak = 1; // Tracks the streak of the most recent occurrences

            for (int i = 1; i < orderedEvents.Count; i++)
            {
                // Check if the event occurred on the previous consecutive day
                if ((orderedEvents[i - 1].Timestamp.Date - orderedEvents[i].Timestamp.Date).TotalDays == 1)
                {
                    currentStreak++;
                }
                else
                {
                    // Stop as soon as a gap is found
                    break;
                }
            }

            return currentStreak;
        }



        private static Dictionary<(string LocationIdentifier, WatchDogComponentTypes ComponentType, int ComponentId, WatchDogIssueTypes IssueType, int? Phase), int>
        CreateDayBeforeCountLookup(List<WatchDogLogEvent> recordsForDayBeforeScanDate)
        {
            return recordsForDayBeforeScanDate
                .GroupBy(r => (r.LocationIdentifier, r.ComponentType, r.ComponentId, r.IssueType ,r.Phase))
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );
        }

        private static List<WatchDogLogEventWithCountAndDate> ConvertRecords(
            List<WatchDogLogEvent> recordsForScanDate,
            Dictionary<(string LocationIdentifier, WatchDogComponentTypes ComponentType, int ComponentId, WatchDogIssueTypes IssueType, int? Phase),
            (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)> countAndDateLookupForLast12Months)
        {
            return recordsForScanDate
                .Select(r =>
                {
                    // Try to get the data from the dictionary once
                    var key = (r.LocationIdentifier, r.ComponentType, r.ComponentId, r.IssueType,  r.Phase);
                    var found = countAndDateLookupForLast12Months.TryGetValue(key, out var data);

                    return new WatchDogLogEventWithCountAndDate(r.LocationId, r.LocationIdentifier, r.Timestamp, r.ComponentType, r.ComponentId, r.IssueType, r.Details, r.Phase)
                    {
                        EventCount = found ? data.Count : 0,
                        DateOfFirstInstance = found ? data.DateOfFirstOccurrence : r.Timestamp,
                        ConsecutiveOccurenceCount = found ? data.ConsecutiveOccurrenceCount : 1
                    };
                })
                .ToList();
        }


        private static (List<WatchDogLogEventWithCountAndDate> newIssues, List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues, List<WatchDogLogEventWithCountAndDate> recurringIssues)
        CategorizeIssues(List<WatchDogLogEventWithCountAndDate> allConvertedRecords, Dictionary<(string LocationIdentifier, WatchDogComponentTypes ComponentType, int ComponentId, WatchDogIssueTypes IssueType, int? Phase), int> countForDayBeforeScanDate)
        {
            //Errors that have not happened before
            var newIssues = allConvertedRecords
                .Where(r => r.EventCount == 0)
                .ToList();

            //Errors that happened yesterday
            var dailyRecurringIssues = allConvertedRecords
                .Where(r => r.ConsecutiveOccurenceCount > 1)
                //.Where(r => countForDayBeforeScanDate.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType, r.Phase), out var dayBeforeCount) && dayBeforeCount >= 1)
                .ToList();
            //Errors are not new and did not happen yesterday
            var recurringIssues = allConvertedRecords
                .Except(newIssues.Concat(dailyRecurringIssues))
                .Except(newIssues.Concat(newIssues))                
                .ToList();

            return (newIssues, dailyRecurringIssues, recurringIssues);
        }
    }

    public class WatchDogGroup
    {
        public int LocationId { get; set; }
        public WatchDogComponentTypes ComponentType { get; set; }
        public int ComponentId { get; set; }
        public WatchDogIssueTypes IssueType { get; set; }
        public int? Phase { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is WatchDogGroup other)
            {
                return LocationId == other.LocationId &&
                       ComponentType == other.ComponentType &&
                       ComponentId == other.ComponentId &&
                       IssueType == other.IssueType &&
                       Phase == other.Phase;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked // Allow arithmetic overflow, no exceptions
            {
                int hash = 17; // A prime number to start the hash
                hash = hash * 23 + LocationId.GetHashCode();
                hash = hash * 23 + ComponentType.GetHashCode();
                hash = hash * 23 + ComponentId.GetHashCode();
                hash = hash * 23 + IssueType.GetHashCode();
                hash = hash * 23 + (Phase.HasValue ? Phase.Value.GetHashCode() : -1);
                return hash;
            }
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
