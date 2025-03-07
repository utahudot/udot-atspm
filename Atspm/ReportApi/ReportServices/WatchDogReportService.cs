#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/WatchDogReportService.cs
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

using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Repositories;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Preempt request report service
    /// </summary>
    public class WatchDogReportService : ReportServiceBase<WatchDogOptions, WatchDogResult>
    {
        private readonly IWatchDogEventLogRepository watchDogLogEventRepository;
        private readonly ILocationRepository locationRepository;
        private readonly IJurisdictionRepository jurisdictionRepository;
        private readonly IRegionsRepository regionsRepository;
        private readonly WatchDogIgnoreEventService watchDogIgnoreEventService;


        /// <inheritdoc/>
        public WatchDogReportService(IWatchDogEventLogRepository watchDogLogEventRepository, ILocationRepository locationRepository, IJurisdictionRepository jurisdictionRepository, IRegionsRepository regionsRepository, WatchDogIgnoreEventService watchDogIgnoreEventService)

        {
            this.watchDogLogEventRepository = watchDogLogEventRepository;
            this.locationRepository = locationRepository;
            this.jurisdictionRepository = jurisdictionRepository;
            this.regionsRepository = regionsRepository;
            this.watchDogIgnoreEventService = watchDogIgnoreEventService;
        }

        /// <inheritdoc/>
        public override async Task<WatchDogResult> ExecuteAsync(WatchDogOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            IEnumerable<WatchDogLogEvent> query;
            if (parameter.IsFilteredEvents)
            {
                query = watchDogIgnoreEventService.GetFilteredWatchDogEventsForReport(parameter);
            }
            else
            {
                query = watchDogLogEventRepository.GetList()
                .Where(w => w.Timestamp >= parameter.Start && w.Timestamp < parameter.End).ToList();
            }


            if (parameter.LocationIdentifier != null)
            {
                query = query.Where(w => w.LocationIdentifier == parameter.LocationIdentifier);
            }
            if (parameter.IssueType != null)
            {
                query = query.Where(w => (int)w.IssueType == parameter.IssueType.Value);
            }


            var events = query.ToList();

            var distinctLocationIds = events.Select(e => e.LocationId).Distinct();
            var locations = locationRepository.GetList()
                .Include(a => a.Areas)
                .Where(l => distinctLocationIds.Contains(l.Id))
                .ToList();
            if (parameter.AreaId != null)
            {
                var locationById = locations.Where(s => s.Areas.Select(a => a.Id).ToList().Contains(parameter.AreaId.Value)).Select(l => l.Id).ToList();
                events = events.Where(e => locationById.Contains(e.LocationId)).ToList();
            }
            if (parameter.JurisdictionId != null)
            {
                locations = locations.Where(l => l.JurisdictionId == parameter.JurisdictionId).ToList();
                var locationIdsWithMatchingJurisdiction = new HashSet<int>(locations.Select(l => l.Id));
                events = events.Where(e => locationIdsWithMatchingJurisdiction.Contains(e.LocationId)).ToList();
            }
            if (parameter.RegionId != null)
            {
                locations = locations.Where(l => l.RegionId == parameter.RegionId).ToList();
                var locationIdsWithMatchingRegion = new HashSet<int>(locations.Select(l => l.Id));
                events = events.Where(e => locationIdsWithMatchingRegion.Contains(e.LocationId)).ToList();
            }

            var jurisdictions = jurisdictionRepository.GetList();
            var jurisdictionDict = jurisdictions.ToDictionary(j => j.Id, j => j.Name);

            var regions = regionsRepository.GetList();
            var regionsDict = regions.ToDictionary(r => r.Id, r => r.Description);


            var result = new WatchDogResult();
            result.LogEvents = new List<WatchDogLogEventDTO>();
            result.Start = parameter.Start;
            result.End = parameter.End;

            foreach (var e in events)
            {
                var location = locations.Where(l => l.Id == e.LocationId).FirstOrDefault();
                if (location == null)
                {
                    continue;
                }
                string jurisdictionName = jurisdictionDict.ContainsKey(location.JurisdictionId ?? 0)
                                              ? jurisdictionDict[location.JurisdictionId ?? 0]
                                              : "NA";
                string regionDescription = regionsDict.ContainsKey(location.RegionId ?? 0)
                                  ? regionsDict[location.RegionId ?? 0]
                                  : "NA";

                result.LogEvents.Add(
                    new WatchDogLogEventDTO(
                        e.LocationId,
                        e.LocationIdentifier,
                        e.Timestamp,
                        e.ComponentType,
                        e.ComponentId,
                        e.IssueType,
                        e.Details,
                        e.Phase,
                        location.RegionId,
                        regionDescription,
                        location.JurisdictionId,
                        jurisdictionName,
                        location.Areas.Select(a => new AreaDTO { Id = a.Id, Name = a.Name })
                )); ;
            }

            return result;
        }
    }
}