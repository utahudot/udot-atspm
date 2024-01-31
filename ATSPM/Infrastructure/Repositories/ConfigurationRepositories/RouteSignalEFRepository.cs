using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IRouteLocationsRepository"/>
    public class RouteLocationEFRepository : ATSPMRepositoryEFBase<RouteLocation>, IRouteLocationsRepository
    {
        /// <inheritdoc/>
        public RouteLocationEFRepository(ConfigContext db, ILogger<RouteLocationEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<RouteLocation> GetList()
        {
            return base.GetList()
                .Include(i => i.Route)
                .Include(i => i.PrimaryDirection)
                .Include(i => i.OpposingDirection)
                .Include(i => i.PreviousLocationDistance)
                .Include(i => i.NextLocationDistance)
                .OrderBy(o => o.Order);
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
