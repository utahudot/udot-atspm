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
    public static class ApproachRepositoryExtensions
    {
        public static IReadOnlyList<Approach> GetApproachesByIds(this IApproachRepository repo, IEnumerable<int> excludedApproachIds)
        {
            return repo.GetList(w => excludedApproachIds.Contains(w.Id));
        }

        #region Obsolete

        [Obsolete("Use Add instead")]
        public static void AddOrUpdate(this IApproachRepository repo, Approach approach)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This Method is not used")]
        public static Approach FindAppoachByVersionIdPhaseOverlapAndDirection(this IApproachRepository repo, int versionId, int phaseNumber, bool isOverlap, int directionTypeId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetList instead")]
        public static IReadOnlyList<Approach> GetAllApproaches(this IApproachRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup instead")]
        public static Approach GetApproachByApproachID(this IApproachRepository repo, int approachID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove instead")]
        public static void Remove(int approachID)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
