using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface ISegmentImpactRepository : IAsyncRepository<SegmentImpact>
    {
        Task<IReadOnlyList<SegmentImpact>> GetImpactsForSegmentAsync(Guid segmentId);
        Task<IReadOnlyList<SegmentImpact>> GetSegmentsForImpactAsync(Guid impactId);
        Task RemoveAllImpactsFromSegmentAsync(Guid segmentId);
        Task RemoveAllSegmentsFromImpactIdAsync(Guid? impactId);
    }
}
