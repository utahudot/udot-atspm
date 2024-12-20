using Utah.Udot.Atspm.Business.Watchdog;

namespace Utah.Udot.Atspm.Services
{
    public interface ISegmentedErrorsService
    {
        (List<WatchDogLogEventWithCountAndDate> newIssues, List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues, List<WatchDogLogEventWithCountAndDate> recurringIssues)
        GetSegmentedErrors(List<WatchDogLogEvent> recordsForScanDate, WatchdogEmailOptions emailOptions);
    }
}
