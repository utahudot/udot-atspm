using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Jurisdiction entity framework repository
    /// </summary>
    public class JurisdictionEFRepository : ATSPMRepositoryEFBase<Jurisdiction>, IJurisdictionRepository
    {
        /// <inheritdoc/>
        public JurisdictionEFRepository(ConfigContext db, ILogger<JurisdictionEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<Jurisdiction> GetList()
        {
            return base.GetList().OrderBy(o => o.Name);
        }

        #endregion

        #region IJurisdictionRepository

        #endregion
    }
}
