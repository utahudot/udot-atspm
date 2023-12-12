using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Route signal repository
    /// </summary>
    public interface IRouteSignalsRepository : IAsyncRepository<RouteSignal>
    {
        //TODO: make sure these are still being used
        
        /// <summary>
        /// Get route from signal id
        /// </summary>
        /// <param name="id">id of signal</param>
        /// <returns></returns>
        RouteSignal GetByRoutelocationId(int id);
        
        /// <summary>
        /// Moves signal up in route order
        /// </summary>
        /// <param name="routeId">id of route</param>
        /// <param name="routelocationId">id of signal route</param>
        void MoveRouteSignalUp(int routeId, int routelocationId);
        
        /// <summary>
        /// Moves signal down in route order
        /// </summary>
        /// <param name="routeId">id of route</param>
        /// <param name="routelocationId">id of signal route</param>
        void MoveRouteSignalDown(int routeId, int routelocationId);

        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<RouteSignal> GetAllRoutesDetails();

        //[Obsolete("Use Lookup instead")]
        //IReadOnlyList<RouteSignal> GetByRouteID(int routeID);

        //[Obsolete("Use Delete in the BaseClass")]
        //void DeleteByRouteID(int routeID);

        //[Obsolete("Use Delete in the BaseClass")]
        //void DeleteById(int id);

        //[Obsolete("Use Update in the BaseClass")]
        //void UpdateByRouteAndApproachID(int routeID, string locationId, int newOrderNumber);

        //[Obsolete("Use Add in the BaseClass")]
        //void Add(RouteSignal newRouteDetail);

        #endregion
    }
}
