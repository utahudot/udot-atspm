using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Route entity framework repository
    /// </summary>
    public class RouteEFRepository : ATSPMRepositoryEFBase<Route>, IRouteRepository
    {
        /// <inheritdoc/>
        public RouteEFRepository(ConfigContext db, ILogger<RouteEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IRouteRepository

        #endregion
    }
}
