using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Route Location repository
    /// </summary>
    public interface IRouteLocationsRepository : IAsyncRepository<RouteLocation>
    {
        //TODO: make sure these are still being used

        /// <summary>
        /// Get route from Location id
        /// </summary>
        /// <param name="id">id of Location</param>
        /// <returns></returns>
        RouteLocation GetByRoutelocationId(int id);

        /// <summary>
        /// Moves Location up in route order
        /// </summary>
        /// <param name="routeId">id of route</param>
        /// <param name="routelocationId">id of Location route</param>
        void MoveRouteLocationUp(int routeId, int routelocationId);

        /// <summary>
        /// Moves Location down in route order
        /// </summary>
        /// <param name="routeId">id of route</param>
        /// <param name="routelocationId">id of Location route</param>
        void MoveRouteLocationDown(int routeId, int routelocationId);

        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<RouteLocation> GetAllRoutesDetails();

        //[Obsolete("Use Lookup instead")]
        //IReadOnlyList<RouteLocation> GetByRouteID(int routeID);

        //[Obsolete("Use Delete in the BaseClass")]
        //void DeleteByRouteID(int routeID);

        //[Obsolete("Use Delete in the BaseClass")]
        //void DeleteById(int id);

        //[Obsolete("Use Update in the BaseClass")]
        //void UpdateByRouteAndApproachID(int routeID, string locationId, int newOrderNumber);

        //[Obsolete("Use Add in the BaseClass")]
        //void Add(RouteLocation newRouteDetail);

        #endregion
    }
}
