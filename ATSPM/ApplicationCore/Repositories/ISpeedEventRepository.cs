using ATSPM.Data.Models;
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
        IReadOnlyList<SpeedEvent> GetSpeedEventsByDetector(DateTime startDate, DateTime endDate, Detector detector,
        int minSpeedFilter);

        IReadOnlyList<SpeedEvent> GetSpeedEventsBySignal(DateTime startDate, DateTime endDate, Approach approach);
    }
}
