using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IDetectorCommentRepository : IAsyncRepository<DetectorComment>
    {
        DetectorComment GetMostRecentDetectorCommentByDetectorID(int ID);

        [Obsolete("Use GetList in the BaseClass")]
        List<DetectorComment> GetAllDetectorComments();

        [Obsolete("Use Lookup in the BaseClass")]
        DetectorComment GetDetectorCommentByDetectorCommentID(int detectorCommentID);

        [Obsolete("Use Add in the BaseClass")]
        void AddOrUpdate(DetectorComment detectorComment);

        [Obsolete("Use Add in the BaseClass")]
        void Add(DetectorComment detectorComment);

        [Obsolete("Use Remove in the BaseClass")]
        void Remove(DetectorComment detectorComment);
    }
}
