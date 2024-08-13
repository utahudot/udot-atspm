using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface ISegmentImpactRepository : IAsyncRepository<SegmentImpact>
    {
        Task<IReadOnlyList<SegmentImpact>> GetImpactsForSegmentAsync(Guid segmentId);
        Task<IReadOnlyList<SegmentImpact>> GetSegmentsForImpactAsync(Guid impactId);
        Task RemoveAllImpactsFromSegmentAsync(Guid segmentId);
        Task RemoveAllSegmentsFromImpactIdAsync(Guid? impactId);
    }
}
