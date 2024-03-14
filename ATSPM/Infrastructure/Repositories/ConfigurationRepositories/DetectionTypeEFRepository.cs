using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IDetectionTypeRepository"/>
    public class DetectionTypeEFRepository : ATSPMRepositoryEFBase<DetectionType>, IDetectionTypeRepository
    {
        /// <inheritdoc/>
        public DetectionTypeEFRepository(ConfigContext db, ILogger<DetectionTypeEFRepository> log) : base(db, log) { }

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
