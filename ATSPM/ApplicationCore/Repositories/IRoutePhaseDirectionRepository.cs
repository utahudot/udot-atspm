using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IRoutePhaseDirectionRepository : IAsyncRepository<RoutePhaseDirection>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyList<RoutePhaseDirection> GetAll();
        
        [Obsolete("Use Lookup instead")]
        RoutePhaseDirection GetByID(int routeID);
        
        [Obsolete("Use Delete in the BaseClass")]
        void DeleteByID(int id);
        
        [Obsolete("Use Update in the BaseClass")]
        void Update(RoutePhaseDirection routePhaseDirection);
        
        [Obsolete("Use Add in the BaseClass")]
        void Add(RoutePhaseDirection newRRoutePhaseDirection);
    }
}
