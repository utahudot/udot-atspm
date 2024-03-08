using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.EventLogRepositories
{
    ///<inheritdoc cref="ISpeedEventLogRepository"/>
    public class SpeedEventLogEFRepository : EventLogEFRepositoryBase<SpeedEvent>, ISpeedEventLogRepository
    {
        ///<inheritdoc/>
        public SpeedEventLogEFRepository(EventLogContext db, ILogger<SpeedEventLogEFRepository> log) : base(db, log) { }

        #region ISpeedEventLogRepository

        #endregion
    }
}
