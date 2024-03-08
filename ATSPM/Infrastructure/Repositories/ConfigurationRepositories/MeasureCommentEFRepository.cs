using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IMeasureCommentRepository"/>
    public class MeasureCommentEFRepository : ATSPMRepositoryEFBase<MeasureComment>, IMeasureCommentRepository
    {
        /// <inheritdoc/>
        public MeasureCommentEFRepository(ConfigContext db, ILogger<MeasureCommentEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IMeasureCommentRepository

        #endregion
    }
}
