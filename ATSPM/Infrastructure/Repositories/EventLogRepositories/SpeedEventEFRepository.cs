using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.EventLogRepositories
{
    public class SpeedEventEFRepository : ATSPMRepositoryEFBase<OldSpeedEvent>, ISpeedEventRepository
    {
        public SpeedEventEFRepository(SpeedContext db, ILogger<SpeedEventEFRepository> log) : base(db, log) { }

        #region ISpeedEventRepository

        #endregion
    }
}
