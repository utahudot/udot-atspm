using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Route repository
    /// </summary>
    public interface IRouteRepository : IAsyncRepository<Route>
    {
        //TODO: verify if this is needed or not
        //Route GetRouteByIdAndDate(int routeId, DateTime startDate);

        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Route> GetAllRoutes();

        //[Obsolete("Use Lookup instead")]
        //Route GetRouteByID(int routeID);

        //[Obsolete("Use Lookup instead")]
        //Route GetRouteByName(string routeName);

        //[Obsolete("Use Delete in the BaseClass")]
        //void DeleteByID(int routeID);

        //[Obsolete("Use Update in the BaseClass")]
        //void Update(Route route);

        //[Obsolete("Use Add in the BaseClass")]
        //void Add(Route newRoute);

        #endregion
    }
}
