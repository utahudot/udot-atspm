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
            throw new NotImplementedException();
        }

        public RoutePhaseDirection GetByID(int routeID)
        {
            throw new NotImplementedException();
        }
    }
}
