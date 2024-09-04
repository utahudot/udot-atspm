using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Infrastructure.Repositories;
using Utah.Udot.Atspm.Repositories;

namespace Utah.Udot.ATSPM.Infrastructure.Repositories
{
    public class WatchDogIgnoreEventEFRepository : ATSPMRepositoryEFBase<WatchDogIgnoreEvent>, IWatchDogIgnoreEventLogRepository
    {
        public WatchDogIgnoreEventEFRepository(DbContext db, ILogger<ATSPMRepositoryEFBase<WatchDogIgnoreEvent>> log) : base(db, log)
        {
        }
    }
}
