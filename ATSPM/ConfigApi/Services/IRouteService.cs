using ATSPM.Data.Models;
using System.Collections.Generic;


namespace ATSPM.ConfigApi.Services
{
    public interface IRouteService
    {
        void CreateRouteWithLocations(Data.Models.Route route, ICollection<RouteLocation> routeLocations);
    }
}
