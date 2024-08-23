using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface ISegmentEntityRepository : IAsyncRepository<SegmentEntity>
    {
        public Task AddEntitiesAsync(List<SegmentEntity> routeEntities);
        public Task<List<SegmentEntityWithSpeed>> GetEntitiesWithSpeedForSourceId(int sourceId);
        public Task<List<SegmentEntityWithSpeedAndAlternateIdentifier>> GetEntitiesWithSpeedAndAlternateIdentifierForSourceId(int sourceId);
        public Task<List<SegmentEntity>> GetEntitiesForEntityType(string entityType);
    }
}
