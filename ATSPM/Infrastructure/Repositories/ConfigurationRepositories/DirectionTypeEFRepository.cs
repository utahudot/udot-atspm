using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Direction type entity framework repository
    /// </summary>
    public class DirectionTypeEFRepository : ATSPMRepositoryEFBase<DirectionType>, IDirectionTypeRepository
    {
        /// <inheritdoc/>
        public DirectionTypeEFRepository(ConfigContext db, ILogger<DirectionTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<DirectionType> GetList()
        {
            return base.GetList().OrderBy(o => o.DisplayOrder);
        }

        #endregion

        #region IDirectionTypeRepository

        #endregion
    }
}
