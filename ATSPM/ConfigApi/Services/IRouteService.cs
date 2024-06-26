using ATSPM.ConfigApi.Models;


namespace ATSPM.ConfigApi.Services
{
    public interface IRouteService
    {
        Data.Models.Route CreateOrUpdateRoute(RouteDto route);
    }
}
