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
            if (_db.Set<DetectorEventCountAggregation>().Any(r => r.DetectorPrimaryId == detectorId
                                                             && r.BinStartTime >= start && r.BinStartTime <= end))
            {
                return _db.Set<DetectorEventCountAggregation>().Where(r => r.DetectorPrimaryId == detectorId
                                                                  && r.BinStartTime >= start &&
                                                                  r.BinStartTime <= end).ToList();
            }
            return new List<DetectorEventCountAggregation>();
        }

        public int GetDetectorEventCountSumAggregationByDetectorIdAndDateRange(int detectorId, DateTime start, DateTime end)
        {
            var count = 0;
            
            if (_db.Set<DetectorEventCountAggregation>().Any(r => r.DetectorPrimaryId == detectorId
                                                            && r.BinStartTime >= start && r.BinStartTime <= end))
            {
                count = _db.Set<DetectorEventCountAggregation>().Where(r => r.DetectorPrimaryId == detectorId
                                                                      && r.BinStartTime >= start && r.BinStartTime <= end)
                    .Sum(r => r.EventCount);
            }

            return count;
        }
    }
}
