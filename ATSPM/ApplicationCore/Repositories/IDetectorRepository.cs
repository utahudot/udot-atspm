using ATSPM.Data.Enums;
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
    /// Dectector Repository
    /// </summary>
    public interface IDetectorRepository : IAsyncRepository<Detector>
    {
        /// <summary>
        /// Checks to see if <see cref="Detector"/>.<see cref="DetectionType"/> metrics contains <paramref name="metricId"/>
        /// </summary>
        /// <param name="detector"><see cref="Detector"/> whos <see cref="DetectionType"/> collection to check</param>
        /// <param name="metricId"><see cref="MetricType"/> id to match within <see cref="DetectionType"/> collection</param>
        /// <returns></returns>
        bool CheckReportAvialbility(Detector detector, int metricId);

        //TODO: this needs to be moved out of this repo
        //TODO: Figure out why SignalId isn't being used in the implmentation
        /// <summary>
        /// Gets a list of <see cref="Detector"/> that match <paramref name="directionType"/> and <paramref name="movementTypeIds"/>
        /// </summary>
        /// <param name="signalId">Don't know why but this isn't being used???</param>
        /// <param name="directionType">Direction type to match to <see cref="Detector"/></param>
        /// <param name="movementTypeIds">Movement type to match to <see cref="Detector"/></param>
        /// <returns></returns>
        IReadOnlyList<Detector> GetDetectorsBySignalIdMovementTypeIdDirectionTypeId(string signalId, DirectionTypes directionType, List<MovementTypes> movementTypeIds);

        //TODO: this needs to be moved out of this repo
        /// <summary>
        /// Get maximum detector channel from signal controller
        /// </summary>
        /// <param name="id">Signal controller id</param>
        /// <returns></returns>
        int GetMaximumDetectorChannel(int id);

        #region ExtensionMethods

        #endregion

        #region Obsolete

        //[Obsolete("This method is not used", true)]
        //IReadOnlyList<Detector> GetDetectorsBySignalIDAndMetricType(string signalId, int metricId);

        //[Obsolete("User CheckReportAvialbility(Detector detector, int metricId) instead", true)]
        //bool CheckReportAvialbility(string detectorID, int metricId);

        //[Obsolete("User GetList() instead", true)]
        //Detector GetDetectorByDetectorID(string DetectorID);

        //[Obsolete("User GetList() instead", true)]
        //IReadOnlyList<Detector> GetDetectorsBySignalID(string SignalID);

        //[Obsolete("Use Lookup instead", true)]
        //Detector GetDetectorByID(int ID);

        //[Obsolete("Use Add in the BaseClass", true)]
        //Detector Add(Detector Detector);

        //[Obsolete("Use Update in the BaseClass", true)]
        //void Update(Detector Detector);

        //[Obsolete("Use Remove in the BaseClass", true)]
        //void Remove(Detector Detector);

        //[Obsolete("Use Remove in the BaseClass", true)]
        //void Remove(int ID);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Detector> GetDetectorsByIds(List<int> excludedDetectorIds);

        #endregion
    }
}
