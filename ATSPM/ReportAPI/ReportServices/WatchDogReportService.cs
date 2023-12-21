using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PreemptServiceRequest;
using ATSPM.ReportApi.Business.Watchdog;
using ATSPM.ReportApi.TempExtensions;
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

        //research Dependance injection DAN KIMBALL
        // handle returning correct region, area, etc (where nulls are at)
        //handle null params

        /// <inheritdoc/>
        public WatchDogReportService(IWatchDogLogEventRepository watchDogLogEventRepository, ILocationRepository locationRepository)

        {
            this.watchDogLogEventRepository = watchDogLogEventRepository;
            this.locationRepository = locationRepository;
        }

        /// <inheritdoc/>
        public override async Task<WatchDogResult> ExecuteAsync(WatchDogOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {

            //inject watchdog repo 
            var query = watchDogLogEventRepository.GetList()
                .Where(w => w.Timestamp >= parameter.Start && w.Timestamp <= parameter.End);
            if (parameter.LocationId != null)
            {
                query.Where(w => w.locationId == parameter.LocationId);
            }
   

            // do that for all the rest of the params except area, region, jur (because not built out yet.) 

            // get location info. (do we add these to the logobjects itself ask christian)


            var events = query.ToList();


            var distinctLocationIds = events.Select(e => e.locationId).Distinct();
            var locations = locationRepository.GetList()
                .Where(l => distinctLocationIds.Contains(l.Id))
                .ToList();
            if (parameter.AreaId != null)
            {
                var locationById = locations.Where(s => s.Areas.Select(a => a.Id).ToList().Contains(parameter.AreaId.Value)).Select(l => l.Id).ToList();
                events = events.Where(e => locationById.Contains(e.Id)).ToList();

            }

            var result = new WatchDogResult();
            result.LogEvents = new List<WatchDogLogEventDTO>();
            result.Start = parameter.Start;
            result.End = parameter.End; 
            foreach (var e in events)
            {
                var location = locations.Where(l => l.Id == e.locationId).FirstOrDefault();

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
                        location.JurisdictionId
                )); ;
            }

            return result;  
        }
    }
}
