using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IRegionsRepository"/>
    public class RegionEFRepository : ATSPMRepositoryEFBase<Region>, IRegionsRepository
    {
        /// <inheritdoc/>
        public RegionEFRepository(ConfigContext db, ILogger<RegionEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<Region> GetList()
        {
            return base.GetList().OrderBy(o => o.Description);
        }

        #endregion

        #region IRegionsRepository

        #endregion
    }
}
