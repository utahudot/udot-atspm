using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
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
