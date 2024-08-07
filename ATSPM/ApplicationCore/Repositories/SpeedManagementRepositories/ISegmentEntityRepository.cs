using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface ISegmentEntityRepository : IAsyncRepository<SegmentEntity>
    {
        public Task AddEntitiesAsync(List<SegmentEntity> routeEntities);
        public Task<List<SegmentEntityWithSpeed>> GetEntitiesWithSpeedForSourceId(int sourceId);
        public Task<List<SegmentEntityWithSpeedAndAlternateIdentifier>> GetEntitiesWithSpeedAndAlternateIdentifierForSourceId(int sourceId);
        public Task<List<SegmentEntity>> GetEntitiesForEntityType(string entityType);
    }
}
