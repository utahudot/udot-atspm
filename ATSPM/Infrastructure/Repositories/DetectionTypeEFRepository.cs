using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Detection type entity framework repository
    /// </summary>
    public class DetectionTypeEFRepository : ATSPMRepositoryEFBase<DetectionType>, IDetectionTypeRepository
    {
        /// <inheritdoc/>
        public DetectionTypeEFRepository(DbContext db, ILogger<DetectionTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<DetectionType> GetList()
        {
            return base.GetList().OrderBy(o => o.DisplayOrder);
        }

        #endregion

        #region IDetectionTypeRepository

        public IReadOnlyList<DetectionType> GetAllDetectionTypesNoBasic()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
