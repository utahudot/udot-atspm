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
    public class ApproachSpeedAggregationEFRepository : ATSPMRepositoryEFBase<ApproachSpeedAggregation>, IApproachSpeedAggregationRepository
    {

        public ApproachSpeedAggregationEFRepository(DbContext db, ILogger<ApproachSpeedAggregationEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<ApproachSpeedAggregation> GetSpeedsByApproachIDandDateRange(int approachId, DateTime start, DateTime end)
        {
            return _db.Set<ApproachSpeedAggregation>().Where(asa => asa.ApproachId == approachId && asa.BinStartTime >= start && asa.BinStartTime <= end).ToList();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
