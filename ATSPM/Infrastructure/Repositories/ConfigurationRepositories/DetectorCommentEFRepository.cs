using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IDetectorCommentRepository"/>
    public class DetectorCommentEFRepository : ATSPMRepositoryEFBase<DetectorComment>, IDetectorCommentRepository
    {
        /// <inheritdoc/>
        public DetectorCommentEFRepository(ConfigContext db, ILogger<DetectorCommentEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<DetectorComment> GetList()
        {
            return base.GetList()
                .Include(i => i.Detector);
        }

        #endregion

        #region IDetectorCommentRepository

        #endregion
    }
}
