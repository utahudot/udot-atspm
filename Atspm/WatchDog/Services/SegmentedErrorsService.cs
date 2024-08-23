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

            //Create a dictonary to allow for quick search on last 12 months records
            var locationIssueLookupForLast12Months = recordsForLast12Months
            .GroupBy(r => r.LocationIdentifier)
            .ToDictionary(
                group => group.Key,
                group => new HashSet<WatchDogIssueTypes>(group.Select(g => g.IssueType))
            );

            //Create a dictonary to allow for quick search on day before scan date
            var dayBeforeLookup = recordsForDayBeforeScanDate
                .GroupBy(r => r.LocationIdentifier)
                .ToDictionary(
                    group => group.Key,
                    group => new HashSet<WatchDogIssueTypes>(group.Select(i => i.IssueType))
                );

            //Filter values from recordsFromDayBefore that are newly occured
            var newIssues = recordsFromDayBefore
                .Where(r =>
                    !locationIssueLookupForLast12Months.ContainsKey(r.LocationIdentifier) ||
                    !locationIssueLookupForLast12Months[r.LocationIdentifier].Contains(r.IssueType)
                )
                .ToList();

            //Filter values from recordsFromDayBefore that have occured twice in a row
            var dailyRecurringIssues = recordsFromDayBefore
                .Where(r =>
                    dayBeforeLookup.ContainsKey(r.LocationIdentifier) &&
                    dayBeforeLookup[r.LocationIdentifier].Contains(r.IssueType)
                )
                .ToList();

            var combinedMatches = newIssues.Concat(dailyRecurringIssues).ToList();

            //remaing values from recordsFromDayBefore
            var filteredList = recordsFromDayBefore.Except(combinedMatches).ToList();

            return (newIssues, dailyRecurringIssues, filteredList);
        }
    }
}
