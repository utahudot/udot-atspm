using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IRouteRepository : IAsyncRepository<Route>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<Route> GetAllRoutes();
        [Obsolete("Use Lookup instead")]
        Route GetRouteByID(int routeID);
        [Obsolete("Use Lookup instead")]
        Route GetRouteByName(string routeName);
        [Obsolete("Use Delete in the BaseClass")]
        void DeleteByID(int routeID);
        [Obsolete("Use Update in the BaseClass")]
        void Update(Route route);
        [Obsolete("Use Add in the BaseClass")]
        void Add(Route newRoute);
        Route GetRouteByIDAndDate(int routeId, DateTime startDate);
    }
}
