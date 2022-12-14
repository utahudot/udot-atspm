using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class DetectorCommentRepositoryExtensions
    {
        public static DetectorComment GetMostRecentDetectorCommentByDetectorID(this IDetectorCommentRepository repo, int id)
        {
            return repo.GetList().Where(r => r.DetectorId == id).OrderByDescending(r => r.TimeStamp).FirstOrDefault();
        }

        #region Obsolete

        [Obsolete("Use GetList in the BaseClass", true)]
        public static List<DetectorComment> GetAllDetectorComments(this IDetectorCommentRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup in the BaseClass", true)]
        public static DetectorComment GetDetectorCommentByDetectorCommentID(this IDetectorCommentRepository repo, int detectorCommentID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Add in the BaseClass")]
        public static void AddOrUpdate(this IDetectorCommentRepository repo, DetectorComment detectorComment)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Add in the BaseClass", true)]
        public static void Add(this IDetectorCommentRepository repo, DetectorComment detectorComment)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove in the BaseClass", true)]
        public static void Remove(this IDetectorCommentRepository repo, DetectorComment detectorComment)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
