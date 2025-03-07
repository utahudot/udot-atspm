#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices/WatchDogIgnoreEventService.cs
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
using Utah.Udot.Atspm.Repositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    public class WatchDogIgnoreEventService : IWatchDogIgnoreEventService
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
