using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IDetectionHardwareRepository : IAsyncRepository<DetectionHardware>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyList<DetectionHardware> GetAllDetectionHardwaresNoBasic();

        [Obsolete("Use GetList instead")]
        IReadOnlyList<DetectionHardware> GetAllDetectionHardwares();
        
        [Obsolete("Use Lookup instead")]
        DetectionHardware GetDetectionHardwareByID(int ID);
        
        [Obsolete("Use Update in the BaseClass")]
        void Update(DetectionHardware DetectionHardware);
        
        [Obsolete("Use Add in the BaseClass")]
        void Add(DetectionHardware DetectionHardware);
        
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(DetectionHardware DetectionHardware);
    }
}
