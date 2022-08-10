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
    public class LaneTypeEFRepository : ATSPMRepositoryEFBase<LaneType>, ILaneTypeRepository
    {
        public LaneTypeEFRepository(DbContext db, ILogger<LaneTypeEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<LaneType> GetAllLaneTypes()
        {
            throw new NotImplementedException();
        }

        public LaneType GetLaneTypeByLaneTypeID(int laneTypeID)
        {
            throw new NotImplementedException();
        }
    }
}
