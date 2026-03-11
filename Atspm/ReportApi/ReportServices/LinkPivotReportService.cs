#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/LinkPivotReportService.cs
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

using Utah.Udot.Atspm.Business.LinkPivot;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    public class LinkPivotReportService : ReportServiceBase<LinkPivotOptions, LinkPivotResult>
    {
        private readonly ILocationRepository locationRepository;
        private readonly IRouteLocationsRepository routeLocationsRepository;
        private readonly LinkPivotService linkPivotService;
        private readonly LinkPivotPcdService linkPivotPcdService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;

        public LinkPivotReportService(ILocationRepository locationRepository,
            IRouteLocationsRepository routeLocationsRepository,
            LinkPivotService linkPivotService,
            LinkPivotPcdService linkPivotPcdService,
            IIndianaEventLogRepository controllerEventLogRepository)
        {
            this.locationRepository = locationRepository;
            this.routeLocationsRepository = routeLocationsRepository;
            this.linkPivotService = linkPivotService;
            this.linkPivotPcdService = linkPivotPcdService;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public override async Task<LinkPivotResult> ExecuteAsync(LinkPivotOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            if (routeLocations == null || routeLocations.Count == 0)
            {
                throw new Exception($"No Route Locations configured for route");
            }
            var result = await Task.Run(() => linkPivotService.GetData(parameter, routeLocations));

            return result;
        }

        public async Task<List<LinkPivotForTsd>> GetLinkPivotForTSD(TimeSpaceDiagramOptions options)
        {
            var routeLocations = GetLocationsFromRouteId(options.RouteId);
            if (routeLocations == null || routeLocations.Count == 0)
            {
                throw new Exception($"No Route Locations configured for route");
            }
            var linkPivotOptions = TransformOptions(options);
            linkPivotOptions.CycleLength = GetModeCycleLength(linkPivotOptions, routeLocations);
            var result = await Task.Run(() => linkPivotService.GetData(linkPivotOptions, routeLocations));

            linkPivotOptions.Direction = "Upstream";
            linkPivotOptions.BiasDirection = "Upstream";

            var opposingResult = await Task.Run(() => linkPivotService.GetData(linkPivotOptions, routeLocations));

            var primaryData = new LinkPivotForTsd("Primary", result);
            var opposingData = new LinkPivotForTsd("Opposing", opposingResult);
            return [primaryData, opposingData];
        }

        private LinkPivotOptions TransformOptions(TimeSpaceDiagramOptions options)
        {
            var linkPivotOptions = new LinkPivotOptions()
            {
                RouteId = options.RouteId,
                StartDate = DateOnly.FromDateTime(options.Start),
                StartTime = TimeOnly.FromDateTime(options.Start),
                EndDate = DateOnly.FromDateTime(options.End),
                EndTime = TimeOnly.FromDateTime(options.End),
                Direction = "Downstream",
                BiasDirection = "Downstream",
                Bias = 0,
                DaysOfWeek = [(int)options.Start.DayOfWeek],
            };

            return linkPivotOptions;
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

        private int GetModeCycleLength(LinkPivotOptions options, List<RouteLocation> routeLocations)
        {
            List<int> cycleLengths = new List<int>();
            var locationIdentifiers = routeLocations.Select(i => i.LocationIdentifier).ToList();
            foreach (var locationIdentifier in locationIdentifiers)
            {
                var controllerEventLogs = controllerEventLogRepository
                    .GetEventsBetweenDates(locationIdentifier, options.StartDate.ToDateTime(options.StartTime).AddHours(-12), options.EndDate.ToDateTime(options.EndTime).AddHours(12)).ToList();

                if (controllerEventLogs.IsNullOrEmpty())
                {
                    throw new Exception($"No Controller Event Logs found for Location {locationIdentifier}");
                }
                var programmedCycleForPlan = controllerEventLogs
                    .GetEventsByEventCodes(options.StartDate.ToDateTime(options.StartTime).AddHours(-12), options.EndDate.ToDateTime(options.EndTime).AddHours(12), new List<short>() { 132 });
                var cycleLength = GetEventOverlappingTime(options.StartDate.ToDateTime(options.StartTime), programmedCycleForPlan, "CycleLength").FirstOrDefault();
                if (cycleLength != null)
                    cycleLengths.Add(cycleLength.EventParam);

            }
            int mode = cycleLengths.Any()
                ? cycleLengths
                    .GroupBy(x => x)
                    .OrderByDescending(g => g.Count())
                    .First().Key
                : 90;
            return mode;
        }

        private List<IndianaEvent> GetEventOverlappingTime(DateTime start, IReadOnlyList<IndianaEvent> programmedCycleForPlan, string eventType)
        {
            var planEvent = programmedCycleForPlan.Where(e => e.Timestamp == start).ToList();
            if (planEvent.Count == 0)
            {
                var planEventInTimeSpan = programmedCycleForPlan.Where(e => e.Timestamp < start)
                    ?.GroupBy(log => log.EventCode)
                    ?.Select(group => group.OrderByDescending(e => e.Timestamp).FirstOrDefault())
                    .ToList();

                if (planEventInTimeSpan != null && planEventInTimeSpan.Count != 0)
                    planEvent = planEventInTimeSpan;
            }

            return planEvent.ToList();
        }
    }
}
