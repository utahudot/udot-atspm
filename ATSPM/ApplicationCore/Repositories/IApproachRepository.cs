using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface IApproachRepository : IAsyncRepository<Approach>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyList<Approach> GetAllApproaches();

        [Obsolete("Use Lookup instead")]
        Approach GetApproachByApproachID(int approachID);

        [Obsolete("Use Add instead")]
        void AddOrUpdate(Approach approach);

        [Obsolete("This Method is not used")]
        Approach FindAppoachByVersionIdPhaseOverlapAndDirection(int versionId, int phaseNumber, bool isOverlap,int directionTypeId);

        [Obsolete("Use Remove instead")]
        void Remove(Approach approach);

        [Obsolete("Use Remove instead")]
        void Remove(int approachID);

        [Obsolete("Use GetList instead")]
        IReadOnlyList<Approach> GetApproachesByIds(List<int> excludedApproachIds);
    }
}
