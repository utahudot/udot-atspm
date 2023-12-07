using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    public class SpeedEventEFRepository : ATSPMRepositoryEFBase<SpeedEvent>, ISpeedEventRepository
    {
        public SpeedEventEFRepository(SpeedContext db, ILogger<SpeedEventEFRepository> log) : base(db, log) { }

        #region ISpeedEventRepository

        #endregion
    }
}
