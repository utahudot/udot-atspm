using System.Linq;
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

        public (List<WatchDogLogEvent>, List<WatchDogLogEvent>, List<WatchDogLogEvent>) GetSegmentedErrors(List<WatchDogLogEvent> recordsFromDayBefore, EmailOptions emailOptions)
        {
            //Get all values from last 12 months excluding the scan date value
            //Also get values from day before the scan date
            var recordsForLast12Months = new List<WatchDogLogEvent>();
            var recordsForDayBeforeScanDate = new List<WatchDogLogEvent>();
            if (emailOptions.WeekdayOnly && emailOptions.ScanDate.DayOfWeek == DayOfWeek.Monday)
            {
                recordsForDayBeforeScanDate = watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-3) &&
                                w.Timestamp < emailOptions.ScanDate.AddDays(-2)).ToList();
                recordsForLast12Months = watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-3).AddMonths(-12) &&
                    w.Timestamp < emailOptions.ScanDate.AddDays(-2)).ToList();
            }
            else
            {
                recordsForDayBeforeScanDate = watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-1) &&
                               w.Timestamp < emailOptions.ScanDate).ToList();
                recordsForLast12Months = watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-1).AddMonths(-12) &&
                    w.Timestamp < emailOptions.ScanDate).ToList();
            }

            var locationIssueLookupForLast12Months = GetUniqeWatchdogEvents(recordsForLast12Months);

            //Create a dictonary to allow for quick search on day before scan date
            var dayBeforeLookup = GetUniqeWatchdogEvents(recordsForDayBeforeScanDate);

            //Filter values from recordsFromDayBefore that are newly occured
            var newIssues = recordsFromDayBefore
                .Where(r =>
                    !locationIssueLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType), out var phase) ||
                    phase != r.Phase
                )
                .ToList();

            //Filter values from recordsFromDayBefore that have occured twice in a row
            var dailyRecurringIssues = recordsFromDayBefore
                .Where(r =>
                    dayBeforeLookup.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType), out var phase) &&
                    phase == r.Phase
                )
                .ToList();

            var combinedMatches = newIssues.Concat(dailyRecurringIssues).ToList();

            //remaing values from recordsFromDayBefore
            var filteredList = recordsFromDayBefore.Except(combinedMatches).ToList();

            return (newIssues, dailyRecurringIssues, filteredList);
        }

        private static Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType), int?> GetUniqeWatchdogEvents(List<WatchDogLogEvent> watchDogRecords)
        {
            //Create a dictonary to allow for quick search
            return watchDogRecords
            .GroupBy(r => (r.LocationIdentifier, r.IssueType, r.ComponentType))
            .ToDictionary(
                group => group.Key,
                group => group.Select(e => e.Phase).FirstOrDefault()
            );
        }
    }
}
