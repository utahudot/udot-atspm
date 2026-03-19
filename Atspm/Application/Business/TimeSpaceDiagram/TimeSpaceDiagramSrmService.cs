#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeSpaceDiagram/TimeSpaceDiagramSrmService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.Atspm.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramSrmService
    {
        private readonly IRouteLocationsRepository routeLocationsRepository;
        private readonly IRouteRepository routeRepository;
        private readonly ITimeSpaceDiagramSrmSource timeSpaceDiagramSrmSource;

        public TimeSpaceDiagramSrmService(
            IRouteLocationsRepository routeLocationsRepository,
            IRouteRepository routeRepository,
            ITimeSpaceDiagramSrmSource timeSpaceDiagramSrmSource)
        {
            this.routeLocationsRepository = routeLocationsRepository;
            this.routeRepository = routeRepository;
            this.timeSpaceDiagramSrmSource = timeSpaceDiagramSrmSource;
        }

        public List<TimeSpaceDiagramSrmPhaseOverlay> GetOverlayData(TimeSpaceDiagramSrmOptions parameter)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            var routeName = GetRouteNameFromId(parameter.RouteId);
            var routeLabel = GetRouteLabel(parameter.RouteId, routeName);

            if (routeLocations.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No route locations are configured for {routeLabel}. Add at least one route location before running this report.");
            }

            var srmTracks = timeSpaceDiagramSrmSource.GetTracks(
                parameter.Start,
                parameter.End,
                routeLocations,
                parameter.SrmCsvContentBase64);

            routeLocations.Sort((r1, r2) => r1.Order - r2.Order);

            var overlays = new List<TimeSpaceDiagramSrmPhaseOverlay>();

            for (int i = 0; i < routeLocations.Count; i++)
            {
                overlays.Add(new TimeSpaceDiagramSrmPhaseOverlay
                {
                    LocationIdentifier = routeLocations[i].LocationIdentifier,
                    PhaseType = "Primary",
                    Order = i,
                    SrmEntityTracks = TimeSpaceDiagramSrmTrackMapper.GetTracksForPhase(
                        routeLocations[i],
                        srmTracks,
                        "Primary",
                        isFirstElement: i == 0,
                        isLastElement: i == routeLocations.Count - 1)
                });
            }

            for (int i = routeLocations.Count - 1, j = 0; i >= 0; i--, j++)
            {
                overlays.Add(new TimeSpaceDiagramSrmPhaseOverlay
                {
                    LocationIdentifier = routeLocations[i].LocationIdentifier,
                    PhaseType = "Opposing",
                    Order = j,
                    SrmEntityTracks = TimeSpaceDiagramSrmTrackMapper.GetTracksForPhase(
                        routeLocations[i],
                        srmTracks,
                        "Opposing",
                        isFirstElement: i == routeLocations.Count - 1,
                        isLastElement: i == 0)
                });
            }

            return overlays;
        }

        private string GetRouteNameFromId(int routeId)
        {
            var routeName = routeRepository.GetList().Where(r => r.Id == routeId)?.FirstOrDefault()?.Name;
            return routeName != null ? routeName : "";
        }

        private static string GetRouteLabel(int routeId, string routeName)
        {
            return string.IsNullOrWhiteSpace(routeName)
                ? $"route id {routeId}"
                : $"route '{routeName}' (id {routeId})";
        }

        private List<RouteLocation> GetLocationsFromRouteId(int routeId)
        {
            var routeLocations = routeLocationsRepository.GetList()
                .Where(l => l.RouteId == routeId)
                .ToList();
            return routeLocations;
        }
    }
}
