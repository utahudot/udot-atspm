using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    public class WatchDogLogEventEFRepository : ATSPMRepositoryEFBase<WatchDogLogEvent>, IWatchDogLogEventRepository
    {
        public WatchDogLogEventEFRepository(ConfigContext db, ILogger<WatchDogLogEventEFRepository> log) : base(db, log) { }


    }
}