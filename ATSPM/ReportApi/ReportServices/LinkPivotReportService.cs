using ATSPM.Application.Business;
using ATSPM.Application.Business.LinkPivot;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;

namespace ATSPM.ReportApi.ReportServices
{
    public class LinkPivotReportService : ReportServiceBase<LinkPivotOptions, LinkPivotResult>
    {
        private readonly ILocationRepository locationRepository;
        private readonly IRouteLocationsRepository routeLocationsRepository;
        private readonly LinkPivotService linkPivotService;
        private readonly LinkPivotPcdService linkPivotPcdService;

        public LinkPivotReportService(ILocationRepository locationRepository,
            IRouteLocationsRepository routeLocationsRepository,
            LinkPivotService linkPivotService,
            LinkPivotPcdService linkPivotPcdService)
        {
            this.locationRepository = locationRepository;
            this.routeLocationsRepository = routeLocationsRepository;
            this.linkPivotService = linkPivotService;
            this.linkPivotPcdService = linkPivotPcdService;
        }

        public override async Task<LinkPivotResult> ExecuteAsync(LinkPivotOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            var result = await Task.Run(() => linkPivotService.GetData(parameter, routeLocations));

            return result;
        }

        public async Task<LinkPivotPcdResult> GetPcdData(LinkPivotPcdOptions options)
        {
            var result = await Task.Run(() => linkPivotPcdService.GetData(options));

            return result;
        }

        public List<Location> FillSignals(int routeId)
        {
            var routeLocations = GetLocationsFromRouteId(routeId);

            List<Location> locations = new List<Location>();
            foreach (var routeSignal in routeLocations)
            {
                var location = locationRepository.GetLatestVersionOfLocation(routeSignal.LocationIdentifier);
                locations.Add(location);
            }
            return locations;
        }


        private List<RouteLocation> GetLocationsFromRouteId(int routeId)
        {
            var routeLocations = routeLocationsRepository.GetList().Where(l => l.RouteId == routeId).ToList();
            return routeLocations ?? new List<RouteLocation>();
        }
    }
}
