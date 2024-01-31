using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IAreaRepository"/>
    public class AreaEFRepository : ATSPMRepositoryEFBase<Area>, IAreaRepository
    {
        public AreaEFRepository(ConfigContext db, ILogger<AreaEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<Area> GetList()
        {
            return base.GetList().OrderBy(o => o.Name);
        }

        #endregion

        #region IAreaRepository

        #endregion
    }
}
