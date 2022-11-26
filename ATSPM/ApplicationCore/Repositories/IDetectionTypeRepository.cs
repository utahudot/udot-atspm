using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface IDetectionTypeRepository //: IAsyncRepository<DetectionType>
    {
        List<DetectionType> GetAllDetectionTypes();

        List<DetectionType> GetAllDetectionTypesNoBasic();

        //List<Models.Repositories.DetectionTypeRepository.DetectetorWithMetricAbbreviation> GetAllDetectionTypesWithSupportedMetricAbbreviations();
        DetectionType GetDetectionTypeByDetectionTypeID(int detectionTypeID);

        [Obsolete("Exists in base class")]
        void Update(DetectionType detectionType);

        [Obsolete("Exists in base class")]
        void Add(DetectionType detectionType);

        [Obsolete("Exists in base class")]
        void Remove(DetectionType detectionType);
    }
}
