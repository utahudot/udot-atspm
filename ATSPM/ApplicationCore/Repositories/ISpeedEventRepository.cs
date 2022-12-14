using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Speed Event Repository
    /// </summary>
    public interface ISpeedEventRepository : IAsyncRepository<SpeedEvent>
    {
        #region ExtensionMethods

        //IReadOnlyList<SpeedEvent> GetSpeedEventsByDetector(Detector detector, DateTime startDate, DateTime endDate, int minSpeedFilter = 5);

        #endregion

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<Speed_Events> GetSpeedEventsBySignal(DateTime startDate, DateTime endDate, Approach approach);

        #endregion
    }
}
