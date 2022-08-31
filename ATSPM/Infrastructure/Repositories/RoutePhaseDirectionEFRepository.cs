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
    public class RoutePhaseDirectionEFRepository : ATSPMRepositoryEFBase<RoutePhaseDirection>, IRoutePhaseDirectionRepository
    {
        public RoutePhaseDirectionEFRepository(DbContext db, ILogger<RoutePhaseDirectionEFRepository> log) : base(db, log)
        {

        }

        public void DeleteByID(int id)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<RoutePhaseDirection> GetAll()
        {
            return _db.Set<RoutePhaseDirection>().ToList();
        }

        public RoutePhaseDirection GetByID(int routeID)
        {
            return _db.Set<RoutePhaseDirection>().Where(r => r.Id == routeID).FirstOrDefault();
        }

        private void CheckForExistingApproach(RoutePhaseDirection newRoutePhaseDirection)
        {
            var routePhaseDirection = _db.Set<RoutePhaseDirection>().Where(r =>
                r.RouteSignalId == newRoutePhaseDirection.RouteSignalId &&
                r.IsPrimaryApproach == newRoutePhaseDirection.IsPrimaryApproach).FirstOrDefault();
            if (routePhaseDirection != null)
            {
                _db.Set<RoutePhaseDirection>().Remove(routePhaseDirection);
                _db.SaveChanges();
            }
        }
    }
}
