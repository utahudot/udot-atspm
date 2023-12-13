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
    public class RouteLocationEFRepository : ATSPMRepositoryEFBase<RouteLocation>, IRouteLocationsRepository
    {
        /// <inheritdoc/>
        public RouteLocationEFRepository(ConfigContext db, ILogger<RouteLocationEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<RouteLocation> GetList()
        {
            return base.GetList().OrderBy(o => o.Order);
        }

        #endregion

        #region IRouteLocationsRepository

        public RouteLocation GetByRoutelocationId(int id)
        {
            throw new System.NotImplementedException();
        }

        public void MoveRouteLocationDown(int routeId, int routelocationId)
        {
            throw new System.NotImplementedException();
        }

        public void MoveRouteLocationUp(int routeId, int routelocationId)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
