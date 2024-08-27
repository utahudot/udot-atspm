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

        private Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType), (int Count, DateTime DateOfFirstOccurrence)>
        CreateCountAndDateLookup(List<WatchDogLogEvent> recordsForLast12Months)
        {
            return recordsForLast12Months
                .GroupBy(r => (r.LocationIdentifier, r.IssueType, r.ComponentType))
                .ToDictionary(
                    group => group.Key,
                    group => (
                        Count: group.Count(),
                        DateOfFirstOccurrence: group.Min(e => e.Timestamp)
                    )
                );
        }

        private Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType), int>
        CreateDayBeforeCountLookup(List<WatchDogLogEvent> recordsForDayBeforeScanDate)
        {
            return recordsForDayBeforeScanDate
                .GroupBy(r => (r.LocationIdentifier, r.IssueType, r.ComponentType))
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );
        }

        private List<WatchDogLogEventWithCountAndDate>
        ConvertRecords(List<WatchDogLogEvent> recordsForScanDate, Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType), (int Count, DateTime DateOfFirstOccurrence)> countAndDateLookupForLast12Months)
        {
            return recordsForScanDate
                .Select(r => new WatchDogLogEventWithCountAndDate(r.LocationId, r.LocationIdentifier, r.Timestamp, r.ComponentType, r.ComponentId, r.IssueType, r.Details, r.Phase)
                {
                    EventCount = countAndDateLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType), out var data) ? data.Count : 0,
                    DateOfFirstInstance = countAndDateLookupForLast12Months.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType), out data) ? data.DateOfFirstOccurrence : r.Timestamp
                })
                .ToList();
        }

        private (List<WatchDogLogEventWithCountAndDate> newIssues, List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues, List<WatchDogLogEventWithCountAndDate> recurringIssues)
        CategorizeIssues(List<WatchDogLogEventWithCountAndDate> allConvertedRecords, Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType), int> countForDayBeforeScanDate)
        {
            var newIssues = allConvertedRecords
                .Where(r => r.EventCount == 0)
                .ToList();

            var dailyRecurringIssues = allConvertedRecords
                .Where(r => countForDayBeforeScanDate.TryGetValue((r.LocationIdentifier, r.IssueType, r.ComponentType), out var dayBeforeCount) && dayBeforeCount == 1)
                .ToList();

            var recurringIssues = allConvertedRecords
                .Except(newIssues.Concat(dailyRecurringIssues))
                .ToList();

            return (newIssues, dailyRecurringIssues, recurringIssues);
        }
    }
}
