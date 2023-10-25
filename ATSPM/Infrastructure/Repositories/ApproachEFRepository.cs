using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Approaches entity framework repository
    /// </summary>
    public class ApproachEFRepository : ATSPMRepositoryEFBase<Approach>, IApproachRepository
    {
        /// <inheritdoc/>
        public ApproachEFRepository(ConfigContext db, ILogger<ApproachEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<Approach> GetList()
        {
            return base.GetList()
                .Include(i => i.DirectionType)
                .Include(i => i.Signal);
                //.Include(i => i.Detectors);
        }

        #endregion

        #region IApproachRepository

        #endregion
    }
}