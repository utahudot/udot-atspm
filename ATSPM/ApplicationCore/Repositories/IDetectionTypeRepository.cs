using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Detection type repository
    /// </summary>
    public interface IDetectionTypeRepository : IAsyncRepository<DetectionType>
    {
        /// <summary>
        /// don't know what this does or if it's needed
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<DetectionType> GetAllDetectionTypesNoBasic();

        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<DetectionType> GetAllDetectionTypes();

        //[Obsolete("Use Lookup in the BaseClass")]
        //DetectionType GetDetectionTypeByDetectionTypeID(int detectionTypeID);

        //[Obsolete("Exists in base class")]
        //void Update(DetectionType detectionType);

        //[Obsolete("Exists in base class")]
        //void Add(DetectionType detectionType);

        //[Obsolete("Exists in base class")]
        //void Remove(DetectionType detectionType);

        #endregion
    }
}
