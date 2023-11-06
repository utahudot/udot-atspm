using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Approach Repository
    /// </summary>
    public interface IApproachRepository : IAsyncRepository<Approach>
    {
        #region ExtensionMethods

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Approach> GetApproachesByIds(List<int> excludedApproachIds);

        #endregion

        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Approach> GetAllApproaches();

        //[Obsolete("Use Lookup instead")]
        //Approach GetApproachByApproachID(int approachID);

        //[Obsolete("Use Add instead")]
        //void AddOrUpdate(Approach approach);

        //[Obsolete("Use Add in the BaseClass")]
        //Approach FindAppoachByVersionIdPhaseOverlapAndDirection(int versionId, int phaseNumber, bool isOverlap, int directionTypeId);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(Approach approach);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(int approachID);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Approach> GetApproachesByIds(List<int> excludedApproachIds);

        #endregion
    }
}
