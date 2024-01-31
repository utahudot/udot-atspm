using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IVersionHistoryRepository"/>
    public class VersionHistoryEFRepository : ATSPMRepositoryEFBase<VersionHistory>, IVersionHistoryRepository
    {
        /// <inheritdoc/>
        public VersionHistoryEFRepository(ConfigContext db, ILogger<VersionHistoryEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<VersionHistory> GetList()
        {
            return base.GetList()
                .Include(i => i.Parent)
                .Include(i => i.Children)
                .OrderBy(o => o.Version);
        }

        #endregion

        #region IVersionHistoryRepository

        #endregion
    }
}
