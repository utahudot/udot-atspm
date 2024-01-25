using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Speed Event Repository
    /// </summary>
    public interface ISpeedEventRepository : IAsyncRepository<OldSpeedEvent>
    {
        #region ExtensionMethods

        //IReadOnlyList<OldSpeedEvent> GetSpeedEventsByDetector(Detector detector, DateTime startDate, DateTime endDate, int minSpeedFilter = 5);

        #endregion

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<Speed_Events> GetSpeedEventsByLocation(DateTime startDate, DateTime endDate, Approach approach);

        #endregion
    }
}
