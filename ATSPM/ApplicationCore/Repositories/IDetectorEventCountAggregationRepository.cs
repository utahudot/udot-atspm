using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    public interface IDetectorEventCountAggregationRepository:IAggregationRepositoryBase
    {
         int GetDetectorEventCountSumAggregationByDetectorIdAndDateRange(int detectorId, DateTime start,
            DateTime end);


        List<DetectorEventCountAggregation> GetDetectorEventCountAggregationByDetectorIdAndDateRange(
            int detectorId, DateTime start,
            DateTime end);

        bool DetectorEventCountAggregationExists(int detectorId, DateTime start,
            DateTime end);
    }
}