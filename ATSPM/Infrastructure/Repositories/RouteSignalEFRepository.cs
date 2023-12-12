using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Location route entity framework repository
    /// </summary>
    public class RouteSignalEFRepository : ATSPMRepositoryEFBase<RouteSignal>, IRouteSignalsRepository
    {
        /// <inheritdoc/>
        public RouteSignalEFRepository(ConfigContext db, ILogger<RouteSignalEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<RouteSignal> GetList()
        {
            return base.GetList().OrderBy(o => o.Order);
        }

        #endregion

        #region IRouteSignalsRepository

        public RouteSignal GetByRoutelocationId(int id)
        {
            throw new System.NotImplementedException();
        }

        public void MoveRouteSignalDown(int routeId, int routelocationId)
        {
            throw new System.NotImplementedException();
        }

        public void MoveRouteSignalUp(int routeId, int routelocationId)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
