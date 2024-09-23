using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface ISegmentRepository : IAsyncRepository<Segment>
    {
        public Task AddRoutesAsync(IEnumerable<Segment> routes);
        public Task AddRouteAsync(Segment route);
        List<Segment> AllSegmentsWithEntity();
        Task<List<Segment>> GetSegmentsDetailsWithEntity(List<Guid> segmentIds);
        Task<List<Segment>> GetSegmentsDetail(List<Guid> segmentIds);
        List<Segment> AllSegmentsWithEntity(int source);
    }
}
