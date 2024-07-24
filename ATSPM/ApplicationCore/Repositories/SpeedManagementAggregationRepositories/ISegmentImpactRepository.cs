using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementAggregationRepositories
{
    public interface ISegmentImpactRepository : IAsyncRepository<SegmentImpact>
    {
        Task<IReadOnlyList<SegmentImpact>> GetSegmentsForImpactAsync(int impactId);
        Task RemoveAllImpactsFromSegmentAsync(int segmentId);
        Task RemoveAllSegmentsFromImpactIdAsync(int? impactId);
    }
}
