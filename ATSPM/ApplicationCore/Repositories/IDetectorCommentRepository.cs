using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Detector Comment Repository
    /// </summary>
    public interface IDetectorCommentRepository : IAsyncRepository<DetectorComment>
    {
        #region ExtensionMethods

        //DetectorComment GetMostRecentDetectorCommentByDetectorID(int ID);

        #endregion

        #region Obsolete

        //[Obsolete("Use GetList in the BaseClass", true)]
        //List<DetectorComment> GetAllDetectorComments();

        //[Obsolete("Use Lookup in the BaseClass", true)]
        //DetectorComment GetDetectorCommentByDetectorCommentID(int detectorCommentID);

        //[Obsolete("Use Add in the BaseClass")]
        //void AddOrUpdate(DetectorComment detectorComment);

        //[Obsolete("Use Add in the BaseClass", true)]
        //void Add(DetectorComment detectorComment);

        //[Obsolete("Use Remove in the BaseClass", true)]
        //void Remove(DetectorComment detectorComment);

        #endregion
    }
}
