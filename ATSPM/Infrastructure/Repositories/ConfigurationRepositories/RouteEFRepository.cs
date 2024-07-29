using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IRouteRepository"/>
    public class RouteEFRepository : ATSPMRepositoryEFBase<Route>, IRouteRepository
    {
        /// <inheritdoc/>
        public RouteEFRepository(ConfigContext db, ILogger<RouteEFRepository> log) : base(db, log) { }
        public IDbContextTransaction BeginTransaction()
        {
            return _db.Database.BeginTransaction();
        }

        #region Overrides

        #endregion

        #region IRouteRepository

        #endregion
    }
}
