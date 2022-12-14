using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface ILaneTypeRepository : IAsyncRepository<LaneType>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyList<LaneType> GetAllLaneTypes();
        
        [Obsolete("Use Lookup instead")]
        LaneType GetLaneTypeByLaneTypeID(int laneTypeID);
        
        [Obsolete("Use Update in the BaseClass")]
        void Update(LaneType laneType);
        
        [Obsolete("Use Add in the BaseClass")]
        void Add(LaneType laneType);
        
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(LaneType laneType);
    }
}
