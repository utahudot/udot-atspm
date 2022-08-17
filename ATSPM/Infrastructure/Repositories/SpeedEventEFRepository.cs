using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class SpeedEventEFRepository : ATSPMRepositoryEFBase<SpeedEvent>, ISpeedEventRepository
    {
        public SpeedEventEFRepository(DbContext db, ILogger<SpeedEventEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<Speed_Events> GetSpeedEventsByDetector(DateTime startDate, DateTime endDate, Detector detector, int minSpeedFilter)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Speed_Events> GetSpeedEventsBySignal(DateTime startDate, DateTime endDate, Approach approach)
        {
            throw new NotImplementedException();
        }
    }
}
