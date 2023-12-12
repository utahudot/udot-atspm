using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Location controller type entity framework repository
    /// </summary>
    public class ControllerTypeEFRepository : ATSPMRepositoryEFBase<ControllerType>, IControllerTypeRepository
    {
        /// <inheritdoc/>
        public ControllerTypeEFRepository(ConfigContext db, ILogger<ControllerTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        //public override IQueryable<ControllerType> GetList()
        //{
        //    return base.GetList()
        //        .Include(i => i.Locations);
        //}

        #endregion

        #region IControllerEventLogRepository

        #endregion
    }
}
