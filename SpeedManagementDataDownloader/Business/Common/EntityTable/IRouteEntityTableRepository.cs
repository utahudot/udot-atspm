using SpeedManagementDataDownloader.Common.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Common.EntityTable
{
    public interface IRouteEntityTableRepository
    {
        Task AddEntitiesAsync(List<RouteEntityTable> routeEntities);
        Task<List<RouteEntityWithSpeed>> GetEntitiesWithSpeedForSourceId(int sourceId);
        Task<List<RouteEntityWithSpeedAndAlternateIdentifier>> GetEntitiesWithSpeedAndAlternateIdentifierForSourceId(int sourceId);
        Task GetEntitiesForEntityType(string entityType);
    }
}
