using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class DetectorEventCountAggregationEFRepository : ATSPMRepositoryEFBase<DetectorEventCountAggregation>, IDetectorEventCountAggregationRepository
    {
        public DetectorEventCountAggregationEFRepository(DbContext db, ILogger<DetectorEventCountAggregationEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<DetectorEventCountAggregation> GetDetectorEventCountAggregationByDetectorIdAndDateRange(int detectorId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public int GetDetectorEventCountSumAggregationByDetectorIdAndDateRange(int detectorId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }
    }
}
