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

        public ApproachSpeedAggregationEFRepository(DbContext db, ILogger<ApproachEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<ApproachSpeedAggregation> GetSpeedsByApproachIDandDateRange(int approachId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
