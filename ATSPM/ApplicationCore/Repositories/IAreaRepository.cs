using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IAreaRepository : IAsyncRepository<Area>
    {
        IReadOnlyList<Area> GetListOfAreasForSignal(string signalId);

        [Obsolete("Use GetList instead")]
        IReadOnlyList<Area> GetAllAreas();
        
        [Obsolete("Use Lookup instead")]
        Area GetAreaByID(int areaId);
        
        [Obsolete("Use Lookup instead")]
        Area GetAreaByName(string AreaName);
        
        [Obsolete("Use Remove in the BaseClass")]
        void DeleteByID(int areaId);
        
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(Area Area);
        
        [Obsolete("Use Update in the BaseClass")]
        void Update(Area newArea);
        
        [Obsolete("Use Add in the BaseClass")]
        void Add(Area newArea);
    }
}
