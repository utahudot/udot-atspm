using ATSPM.ConfigApi.Models;

namespace ATSPM.ConfigApi.Services
{
    public interface IRouteService
    {
        RouteDto UpsertRoute(RouteDto route);

        RouteDto GetRouteWithExpandedLocations(int routeId, bool includeLocationDetail);
    }
}
