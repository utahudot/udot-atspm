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
        public RouteSignalsEFRepository(DbContext db, ILogger<RouteSignalsEFRepository> log) : base(db, log)
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
            return _db.Set<RouteSignal>().ToList();
        }

        public IReadOnlyCollection<RouteSignal> GetByRouteID(int routeID)
        {
            var routes = _db.Set<RouteSignal>().Where(r => r.RouteId == routeID).ToList();

            //if (routes.Count > 0)
            //    return routes;
            //{
            //    var repository =
            //        ApplicationEventRepositoryFactory.Create();
            //    var error = new ApplicationEvent();
            //    error.ApplicationName = "MOE.Common";
            //    error.Class = "Models.Repository.ApproachRouteDetailsRepository";
            //    error.Function = "GetByRouteID";
            //    error.Description = "No Route for ID.  Attempted ID# = " + routeID;
            //    error.SeverityLevel = ApplicationEvent.SeverityLevels.High;
            //    error.Timestamp = DateTime.Now;
            //    repository.Add(error);
            //    throw new Exception("There is no ApproachRouteDetail for this ID");
            //}
        }

        public RouteSignal GetByRouteSignalId(int id)
        {
            throw new NotImplementedException();
        }

        public void MoveRouteSignalDown(int routeId, int routeSignalId)
        {
            var route = _db.Set<Route>().Find(routeId);
            var signal = route.RouteSignals.FirstOrDefault(r => r.Id == routeSignalId);
            var order = signal.Order;
            var swapSignal = route.RouteSignals.FirstOrDefault(r => r.Order == order + 1);
            if (swapSignal != null)
            {
                signal.Order++;
                swapSignal.Order--;
                _db.SaveChanges();
            }
        }

        public void MoveRouteSignalUp(int routeId, int routeSignalId)
        {
            var route = _db.Set<Route>().Find(routeId);
            var signal = route.RouteSignals.FirstOrDefault(r => r.Id == routeSignalId);
            var order = signal.Order;
            var swapSignal = route.RouteSignals.FirstOrDefault(r => r.Order == order - 1);
            if (swapSignal != null)
            {
                signal.Order--;
                swapSignal.Order++;
                _db.SaveChanges();
            }
        }

        public void UpdateByRouteAndApproachID(int routeID, string signalId, int newOrderNumber)
        {
            throw new NotImplementedException();
        }
    }
}
