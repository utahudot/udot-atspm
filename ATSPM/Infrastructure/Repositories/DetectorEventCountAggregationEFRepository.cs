using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class DetectorEventCountAggregationEFRepository : IDetectorEventCountAggregationRepository
    {
        public bool DetectorEventCountAggregationExists(int detectorId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public List<DetectorEventCountAggregation> GetDetectorEventCountAggregationByDetectorIdAndDateRange(int detectorId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public int GetDetectorEventCountSumAggregationByDetectorIdAndDateRange(int detectorId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetLastAggregationDate()
        {
            throw new NotImplementedException();
        }
    }
}
