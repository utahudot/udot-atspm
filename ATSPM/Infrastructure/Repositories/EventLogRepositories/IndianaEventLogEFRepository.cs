using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.EventLogRepositories
{
    ///<inheritdoc cref="IIndianaEventLogRepository"/>
    public class IndianaEventLogEFRepository : EventLogEFRepositoryBase<IndianaEvent>, IIndianaEventLogRepository
    {
        ///<inheritdoc/>
        public IndianaEventLogEFRepository(EventLogContext db, ILogger<IndianaEventLogEFRepository> log) : base(db, log) { }


        #region IIndiannaEventRepository

        #endregion
    }
}
