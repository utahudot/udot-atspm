﻿using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PreemptServiceRequest;
using ATSPM.ReportApi.Business.Watchdog;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Preempt request report service
    /// </summary>
    public class WatchDogReportService : ReportServiceBase<WatchDogOptions, WatchDogResult>
    {
        private readonly IWatchDogLogEventRepository watchDogLogEventRepository;
        private readonly ILocationRepository locationRepository;
        private readonly IJurisdictionRepository jurisdictionRepository;
        private readonly IRegionsRepository regionsRepository;
     

       
        // handle returning correct region, area, etc (where nulls are at)
        //handle null params

        /// <inheritdoc/>
        public WatchDogReportService(IWatchDogLogEventRepository watchDogLogEventRepository, ILocationRepository locationRepository, IJurisdictionRepository jurisdictionRepository, IRegionsRepository regionsRepository)

        {
            this.watchDogLogEventRepository = watchDogLogEventRepository;
            this.locationRepository = locationRepository;
            this.jurisdictionRepository = jurisdictionRepository;
            this.regionsRepository = regionsRepository;
        }

        /// <inheritdoc/>
        public override async Task<WatchDogResult> ExecuteAsync(WatchDogOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {

            //inject watchdog repo 
            var query = watchDogLogEventRepository.GetList()
                .Where(w => w.Timestamp >= parameter.Start && w.Timestamp <= parameter.End);

            if (parameter.LocationIdentifier != null)
            {
               query = query.Where(w => w.locationIdentifier == parameter.LocationIdentifier);
            }
            if (parameter.IssueType != null)
            {
                query = query.Where(w => (int)w.IssueType == parameter.IssueType.Value);
            }



            // do that for all the rest of the params except area, region, jur (because not built out yet.) 

            // get location info. (do we add these to the logobjects itself ask christian)
            

            var events = query.ToList();

         
            var distinctLocationIds = events.Select(e => e.locationId).Distinct();
            var locations = locationRepository.GetList()
                .Include(a => a.Areas)
                .Where(l => distinctLocationIds.Contains(l.Id))
                .ToList();
            if (parameter.AreaId != null)
            {
                var locationById = locations.Where(s => s.Areas.Select(a => a.Id).ToList().Contains(parameter.AreaId.Value)).Select(l => l.Id).ToList();
                events = events.Where(e => locationById.Contains(e.locationId)).ToList();
            }
            if (parameter.JurisdictionId != null)
            {
                locations = locations.Where(l => l.JurisdictionId == parameter.JurisdictionId).ToList();
                var locationIdsWithMatchingJurisdiction = new HashSet<int>(locations.Select(l => l.Id));
                events = events.Where(e => locationIdsWithMatchingJurisdiction.Contains(e.locationId)).ToList();
            }
            if (parameter.RegionId != null)
            {
                locations = locations.Where(l => l.RegionId == parameter.RegionId).ToList();
                var locationIdsWithMatchingRegion = new HashSet<int>(locations.Select(l => l.Id));
                events = events.Where(e => locationIdsWithMatchingRegion.Contains(e.locationId)).ToList();
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
                var location = locations.Where(l => l.Id == e.locationId).FirstOrDefault();
                string jurisdictionName = jurisdictionDict.ContainsKey(location.JurisdictionId)
                                              ? jurisdictionDict[location.JurisdictionId]
                                              : "NA"; // Default value in case JurisdictionId is not found
                string regionDescription = regionsDict.ContainsKey(location.RegionId)
                                  ? regionsDict[location.RegionId]
                                  : "NA"; // Default value in case JurisdictionId is not found 

                result.LogEvents.Add(
                    new WatchDogLogEventDTO(
                        e.locationId,
                        e.locationIdentifier,
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