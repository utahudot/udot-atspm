using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using Route = ATSPM.Data.Models.Route;

namespace ATSPM.ConfigApi.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IRouteLocationsRepository _routeLocationRepository;

        public RouteService(IRouteRepository routeRepository, IRouteLocationsRepository routeLocationRepository)
        {
            _routeRepository = routeRepository;
            _routeLocationRepository = routeLocationRepository;
        }

        public void CreateRouteWithLocations(Route route, ICollection<RouteLocation> routeLocations)
        {
            using IDbContextTransaction transaction = _routeRepository.BeginTransaction();

            try
            {
                _routeRepository.Add(route);

                foreach (var location in routeLocations)
                {
                    location.RouteId = route.Id;
                    _routeLocationRepository.Add(location);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
