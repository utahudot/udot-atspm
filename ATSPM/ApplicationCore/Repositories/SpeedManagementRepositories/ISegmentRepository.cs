using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface ISegmentRepository : IAsyncRepository<Segment>
    {
        public Task AddRoutesAsync(IEnumerable<Segment> routes);
        public Task AddRouteAsync(Segment route);
        List<Segment> AllSegmentsWithEntity();
        Task<List<Segment>> GetSegmentDetails(List<Guid> segmentIds);
    }
}
