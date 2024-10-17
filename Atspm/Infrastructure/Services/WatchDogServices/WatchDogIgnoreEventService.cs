using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Repositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    public class WatchDogIgnoreEventService
    {
        private readonly IWatchDogIgnoreEventRepository watchDogIgnoreEventLogRepository;
        private readonly IWatchDogEventLogRepository watchDogEventLogRepository;

        public WatchDogIgnoreEventService(IWatchDogIgnoreEventRepository watchDogIgnoreEventLogRepository, IWatchDogEventLogRepository watchDogEventLogRepository)
        {
            this.watchDogIgnoreEventLogRepository = watchDogIgnoreEventLogRepository;
            this.watchDogEventLogRepository = watchDogEventLogRepository;
        }

        public List<WatchDogLogEvent> GetFilteredWatchDogEventsForEmail(List<WatchDogLogEvent> watchDogLogEvents, DateTime scanDate)
        {
            //var ignoreEvents = watchDogIgnoreEventLogRepository.GetList();
            var ignoreEvents = watchDogIgnoreEventLogRepository.GetList()
                .Where(ignoreEvent => ignoreEvent.Start <= scanDate && ignoreEvent.End >= scanDate)
                .ToList();

            var result = watchDogLogEvents
                .Where(logEvent => !ignoreEvents.Exists(ignoreEvent =>
                    ignoreEvent.LocationIdentifier == logEvent.LocationIdentifier &&
                    logEvent.Timestamp >= ignoreEvent.Start &&
                    logEvent.Timestamp <= ignoreEvent.End &&
                    (ignoreEvent.ComponentType == null || ignoreEvent.ComponentType == logEvent.ComponentType) &&
                    (ignoreEvent.ComponentId == null || ignoreEvent.ComponentId == logEvent.ComponentId) &&
                    (ignoreEvent.Phase == null || ignoreEvent.Phase == logEvent.Phase)))
                .ToList();

            return result;
        }

        public List<WatchDogLogEvent> GetFilteredWatchDogEventsForReport(WatchDogOptions parameter)
        {
            var ignoreEvents = watchDogIgnoreEventLogRepository.GetList()
                .Where(ignoreEvent => ignoreEvent.End >= parameter.End).ToList();
            var watchDogLogEvents = watchDogEventLogRepository.GetList()
                .Where(w => w.Timestamp >= parameter.Start && w.Timestamp < parameter.End).ToList();

            var result = watchDogLogEvents
                .Where(logEvent => !ignoreEvents.Exists(ignoreEvent =>
                    ignoreEvent.LocationIdentifier == logEvent.LocationIdentifier &&
                    (ignoreEvent.ComponentType == null || ignoreEvent.ComponentType == logEvent.ComponentType) &&
                    (ignoreEvent.ComponentId == null || ignoreEvent.ComponentId == logEvent.ComponentId) &&
                    (ignoreEvent.Phase == null || ignoreEvent.Phase == logEvent.Phase)));

            return result.ToList();
        }
    }
}
