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
    public class DetectionHardwareEFRepository : ATSPMRepositoryEFBase<DetectionHardware>, IDetectionHardwareRepository
    {
        public DetectionHardwareEFRepository(DbContext db, ILogger<DetectionHardwareEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<DetectionHardware> GetAllDetectionHardwares()
        {
            return _db.Set<DetectionHardware>().ToList();
        }

        public IReadOnlyCollection<DetectionHardware> GetAllDetectionHardwaresNoBasic()
        {
            return _db.Set<DetectionHardware>().Where(dh => dh.Name != "Basic").ToList();
        }

        public DetectionHardware GetDetectionHardwareByID(int ID)
        {
            return _db.Set<DetectionHardware>().Where(dh => dh.Id == ID).FirstOrDefault();
        }
    }
}
