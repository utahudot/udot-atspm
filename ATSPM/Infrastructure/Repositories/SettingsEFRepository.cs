using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Settings entity framework repository
    /// </summary>
    public class SettingsEFRepository : ATSPMRepositoryEFBase<Settings>, ISettingsRepository
    {
        /// <inheritdoc/>
        public SettingsEFRepository(ConfigContext db, ILogger<SettingsEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region ISettingsRepository

        #endregion
    }
}