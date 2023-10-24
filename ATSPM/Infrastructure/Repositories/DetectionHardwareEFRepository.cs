using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ATSPM.Infrastructure.Repositories
{
    public class DetectionHardwareEFRepository : ATSPMRepositoryEFBase<DetectionHardware>, IDetectionHardwareRepository
    {
        public DetectionHardwareEFRepository(ConfigContext db, ILogger<DetectionHardwareEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IDetectorCommentRepository

        public IReadOnlyList<DetectionHardware> GetAllDetectionHardwares()
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<DetectionHardware> GetAllDetectionHardwaresNoBasic()
        {
            throw new System.NotImplementedException();
        }

        public DetectionHardware GetDetectionHardwareByID(int ID)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
