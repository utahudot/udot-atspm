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
    public class RouteSignalsEFRepository : ATSPMRepositoryEFBase<RouteSignal>, IRouteSignalsRepository
    {
        public RouteSignalsEFRepository(DbContext db, ILogger<RouteSignalsEFRepository> log)
        {

        }

        public void DeleteById(int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteByRouteID(int routeID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<RouteSignal> GetAllRoutesDetails()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<RouteSignal> GetByRouteID(int routeID)
        {
            throw new NotImplementedException();
        }

        public RouteSignal GetByRouteSignalId(int id)
        {
            throw new NotImplementedException();
        }

        public void MoveRouteSignalDown(int routeId, int routeSignalId)
        {
            throw new NotImplementedException();
        }

        public void MoveRouteSignalUp(int routeId, int routeSignalId)
        {
            throw new NotImplementedException();
        }

        public void UpdateByRouteAndApproachID(int routeID, string signalId, int newOrderNumber)
        {
            throw new NotImplementedException();
        }
    }
}
