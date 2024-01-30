using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Route distance entity framework repository
    /// </summary>
    public class RouteDistanceEFRepository : ATSPMRepositoryEFBase<RouteDistance>, IRouteDistanceRepository
    {
        /// <inheritdoc/>
        public RouteDistanceEFRepository(ConfigContext db, ILogger<RouteDistanceEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<RouteDistance> GetList()
        {
            return base.GetList()
                .Include(i => i.PreviousLocations)
                .Include(i => i.NextLocations);
        }

        #endregion

        #region IRouteDistanceRepository

        /// <inheritdoc/>
        public RouteDistance GetRouteDistanceByLocationIdentifiers(string locationA, string locationB)
        {
            return GetList().FirstOrDefault(w => w.LocationIdentifierA == locationA &&
            w.LocationIdentifierB == locationB ||
            w.LocationIdentifierA == locationB &&
            w.LocationIdentifierB == locationA);
        }

        #endregion
    }
}
