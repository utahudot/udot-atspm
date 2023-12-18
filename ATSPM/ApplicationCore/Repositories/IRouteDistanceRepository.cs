using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Route distance repository
    /// </summary>
    public interface IRouteDistanceRepository : IAsyncRepository<RouteDistance>
    {
        /// <summary>
        /// Gets the <see cref="RouteDistance"/> that contains <paramref name="locationA"/> and <paramref name="locationB"/> 
        /// </summary>
        /// <param name="locationA"></param>
        /// <param name="locationB"></param>
        /// <returns></returns>
        RouteDistance GetRouteDistanceByLocationIdentifiers(string locationA, string locationB);
    }
}
