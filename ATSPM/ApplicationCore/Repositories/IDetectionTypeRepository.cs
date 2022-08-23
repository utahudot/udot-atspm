using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface IDetectionTypeRepository
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
