using ATSPM.ConfigApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ConfigApi.Services
{
    public interface IRouteService
    {
        void CreateOrUpdateRoute(RouteDto route);

        RouteDto GetRouteWithExpandedLocationsAsync(int routeId);
    }
}
