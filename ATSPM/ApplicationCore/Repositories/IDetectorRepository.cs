using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Dectector Repository
    /// </summary>
    public interface IDetectorRepository : IAsyncRepository<Detector>
    {
        //TODO: this needs to be moved out of this repo
        //TODO: Figure out why LocationId isn't being used in the implmentation
        /// <summary>
        /// Gets a list of <see cref="Detector"/> that match <paramref name="directionType"/> and <paramref name="movementTypeIds"/>
        /// </summary>
        /// <param name="locationId">Don't know why but this isn't being used???</param>
        /// <param name="directionType">Direction type to match to <see cref="Detector"/></param>
        /// <param name="movementTypeIds">Movement type to match to <see cref="Detector"/></param>
        /// <returns></returns>
        IReadOnlyList<Detector> GetDetectorsBylocationIdMovementTypeIdDirectionTypeId(string locationId, DirectionTypes directionType, List<MovementTypes> movementTypeIds);

        //TODO: this needs to be moved out of this repo
        /// <summary>
        /// Get maximum detector channel from Location controller
        /// </summary>
        /// <param name="id">Location controller id</param>
        /// <returns></returns>
        int GetMaximumDetectorChannel(int id);

        #region ExtensionMethods

        #endregion

        #region Obsolete

        //[Obsolete("This method is not used", true)]
        //IReadOnlyList<Detector> GetDetectorsBylocationIdAndMetricType(string locationId, int metricId);

        //[Obsolete("User CheckReportAvialbility(Detector detector, int metricId) instead", true)]
        //bool CheckReportAvialbility(string detectorID, int metricId);

        //[Obsolete("User GetList() instead", true)]
        //Detector GetDetectorByDetectorID(string DetectorID);

        //[Obsolete("User GetList() instead", true)]
        //IReadOnlyList<Detector> GetDetectorsBylocationId(string locationId);

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
