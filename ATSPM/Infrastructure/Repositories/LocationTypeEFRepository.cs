using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Location type entity framework repository
    /// </summary>
    public class LocationTypeEFRepository : ATSPMRepositoryEFBase<LocationType>, ILocationTypeRepository
    {
        /// <inheritdoc/>
        public LocationTypeEFRepository(ConfigContext db, ILogger<LocationTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region ILocationTypeRepository

        #endregion
    }
}
