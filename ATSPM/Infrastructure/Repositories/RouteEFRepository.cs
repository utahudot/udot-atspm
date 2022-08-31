using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class RouteEFRepository : ATSPMRepositoryEFBase<Route>, IRouteRepository
    {
        public RouteEFRepository(DbContext db, ILogger<RouteEFRepository> log) : base(db, log)
        {

        }

        public void DeleteByID(int routeID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Route> GetAllRoutes()
        {
            return _db.Set<Route>().ToList();
        }

        public Route GetRouteByID(int routeID)
        {
            throw new NotImplementedException();
        }

        public Route GetRouteByIDAndDate(int routeId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public Route GetRouteByName(string routeName)
        {
            return _db.Set<Route>().Where(r => r.RouteName == routeName).FirstOrDefault();
        }
    }
}
