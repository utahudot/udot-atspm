using ATSPM.ConfigApi.Models;


namespace ATSPM.ConfigApi.Services
{
    public interface IRouteService
    {
        void CreateOrUpdateRoute(RouteDto route);
    }
}
