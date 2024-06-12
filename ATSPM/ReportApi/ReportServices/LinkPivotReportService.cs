#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/LinkPivotReportService.cs
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
