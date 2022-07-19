using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface ISpeedEventRepository : IAsyncRepository<SpeedEvent>
    {
        IReadOnlyCollection<Speed_Events> GetSpeedEventsByDetector(DateTime startDate, DateTime endDate, Detector detector,
            int minSpeedFilter);

        IReadOnlyCollection<Speed_Events> GetSpeedEventsBySignal(DateTime startDate, DateTime endDate, Approach approach);
    }
}
