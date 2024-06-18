using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Models;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ATSPM.ConfigApi.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IRouteLocationsRepository _routeLocationRepository;
        private readonly IRouteDistanceRepository _routeDistanceRepository;

        public RouteService(
            IRouteRepository routeRepository,
            IRouteLocationsRepository routeLocationRepository,
            IRouteDistanceRepository routeDistanceRepository)
        {
            _routeRepository = routeRepository;
            _routeLocationRepository = routeLocationRepository;
            _routeDistanceRepository = routeDistanceRepository;
        }

        public void CreateOrUpdateRoute(RouteDto routeDto)
        {
            // Find existing route or create a new one
            var route = _routeRepository.GetList()
                .Where(r => r.Id == routeDto.Id)
                .Include(r => r.RouteLocations)
                .ThenInclude(rl => rl.PreviousLocationDistance)
                .Include(r => r.RouteLocations)
                .ThenInclude(rl => rl.NextLocationDistance)
                .FirstOrDefault() ?? new Data.Models.Route();

            // Update route properties
            route.Name = routeDto.Name;

            // Update or create RouteLocations
            foreach (var routeLocationDto in routeDto.RouteLocations)
            {
                var existingLocation = route.RouteLocations
                    .FirstOrDefault(rl => rl.Order == routeLocationDto.Order);

                if (existingLocation == null)
                {
                    // Create new RouteLocation
                    var newLocation = new RouteLocation
                    {
                        Order = routeLocationDto.Order,
                        PrimaryPhase = routeLocationDto.PrimaryPhase,
                        OpposingPhase = routeLocationDto.OpposingPhase,
                        PrimaryDirectionId = routeLocationDto.PrimaryDirectionId,
                        OpposingDirectionId = routeLocationDto.OpposingDirectionId,
                        IsPrimaryOverlap = routeLocationDto.IsPrimaryOverlap,
                        IsOpposingOverlap = routeLocationDto.IsOpposingOverlap,
                        PreviousLocationDistanceId = routeLocationDto.PreviousLocationDistanceId,
                        NextLocationDistanceId = routeLocationDto.NextLocationDistanceId,
                        LocationIdentifier = routeLocationDto.LocationIdentifier,
                        RouteId = routeLocationDto.RouteId
                    };

                    HandleDistances(routeLocationDto, newLocation);

                    route.RouteLocations.Add(newLocation);
                }
                else
                {
                    // Update existing RouteLocation
                    existingLocation.PrimaryPhase = routeLocationDto.PrimaryPhase;
                    existingLocation.OpposingPhase = routeLocationDto.OpposingPhase;
                    existingLocation.PrimaryDirectionId = routeLocationDto.PrimaryDirectionId;
                    existingLocation.OpposingDirectionId = routeLocationDto.OpposingDirectionId;
                    existingLocation.IsPrimaryOverlap = routeLocationDto.IsPrimaryOverlap;
                    existingLocation.IsOpposingOverlap = routeLocationDto.IsOpposingOverlap;
                    existingLocation.PreviousLocationDistanceId = routeLocationDto.PreviousLocationDistanceId;
                    existingLocation.NextLocationDistanceId = routeLocationDto.NextLocationDistanceId;
                    existingLocation.LocationIdentifier = routeLocationDto.LocationIdentifier;

                    HandleDistances(routeLocationDto, existingLocation);
                }
            }

            // Save changes
            if (route.Id == 0)
            {
                _routeRepository.Add(route);
            }
            else
            {
                _routeRepository.Update(route);
            }
        }

        private void HandleDistances(RouteLocationDto routeLocationDto, RouteLocation location)
        {
            // Handle PreviousLocationDistance
            if (routeLocationDto.PreviousLocationDistanceId == null && routeLocationDto.PreviousLocationDistance != null)
            {
                var previousDistance = _routeDistanceRepository.GetList()
                    .FirstOrDefault(rd => rd.LocationIdentifierA == routeLocationDto.PreviousLocationDistance.LocationIdentifierA &&
                                          rd.LocationIdentifierB == routeLocationDto.PreviousLocationDistance.LocationIdentifierB);

                if (previousDistance == null)
                {
                    previousDistance = new RouteDistance
                    {
                        Distance = routeLocationDto.PreviousLocationDistance.Distance,
                        LocationIdentifierA = routeLocationDto.PreviousLocationDistance.LocationIdentifierA,
                        LocationIdentifierB = routeLocationDto.PreviousLocationDistance.LocationIdentifierB
                    };
                    _routeDistanceRepository.Add(previousDistance);
                }
                else if (previousDistance.Distance != routeLocationDto.PreviousLocationDistance.Distance)
                {
                    previousDistance.Distance = routeLocationDto.PreviousLocationDistance.Distance;
                    _routeDistanceRepository.Update(previousDistance);
                }
                location.PreviousLocationDistanceId = previousDistance.Id;
                location.PreviousLocationDistance = previousDistance;
            }
            else if (routeLocationDto.PreviousLocationDistanceId != null)
            {
                var existingPreviousDistance = _routeDistanceRepository.Lookup(routeLocationDto.PreviousLocationDistanceId.Value);
                if (existingPreviousDistance != null && existingPreviousDistance.Distance != routeLocationDto.PreviousLocationDistance.Distance)
                {
                    existingPreviousDistance.Distance = routeLocationDto.PreviousLocationDistance.Distance;
                    _routeDistanceRepository.Update(existingPreviousDistance);
                }
            }

            // Handle NextLocationDistance
            if (routeLocationDto.NextLocationDistanceId == null && routeLocationDto.NextLocationDistance != null)
            {
                var nextDistance = _routeDistanceRepository.GetList()
                    .FirstOrDefault(rd => rd.LocationIdentifierA == routeLocationDto.NextLocationDistance.LocationIdentifierA &&
                                          rd.LocationIdentifierB == routeLocationDto.NextLocationDistance.LocationIdentifierB);

                if (nextDistance == null)
                {
                    nextDistance = new RouteDistance
                    {
                        Distance = routeLocationDto.NextLocationDistance.Distance,
                        LocationIdentifierA = routeLocationDto.NextLocationDistance.LocationIdentifierA,
                        LocationIdentifierB = routeLocationDto.NextLocationDistance.LocationIdentifierB
                    };
                    _routeDistanceRepository.Add(nextDistance);
                }
                else if (nextDistance.Distance != routeLocationDto.NextLocationDistance.Distance)
                {
                    nextDistance.Distance = routeLocationDto.NextLocationDistance.Distance;
                    _routeDistanceRepository.Update(nextDistance);
                }
                location.NextLocationDistanceId = nextDistance.Id;
                location.NextLocationDistance = nextDistance;
            }
            else if (routeLocationDto.NextLocationDistanceId != null)
            {
                var existingNextDistance = _routeDistanceRepository.Lookup(routeLocationDto.NextLocationDistanceId.Value);
                if (existingNextDistance != null && existingNextDistance.Distance != routeLocationDto.NextLocationDistance.Distance)
                {
                    existingNextDistance.Distance = routeLocationDto.NextLocationDistance.Distance;
                    _routeDistanceRepository.Update(existingNextDistance);
                }
            }
        }
    }
}
