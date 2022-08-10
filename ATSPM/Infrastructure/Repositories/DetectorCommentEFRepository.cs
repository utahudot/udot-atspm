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
    public class DetectorCommentEFRepository : ATSPMRepositoryEFBase<DetectorComment>, IDetectorCommentRepository
    {
        public DetectorCommentEFRepository(DbContext db, ILogger<ApplicationSettingsEFRepository> log) : base(db, log)
        {

        }

        public void AddOrUpdate(DetectorComment detectorComment)
        {
            throw new NotImplementedException();
        }

        public List<DetectorComment> GetAllDetectorComments()
        {
            throw new NotImplementedException();
        }

        public DetectorComment GetDetectorCommentByDetectorCommentID(int detectorCommentID)
        {
            throw new NotImplementedException();
        }

        public DetectorComment GetMostRecentDetectorCommentByDetectorID(int ID)
        {
            throw new NotImplementedException();
        }
    }
}
