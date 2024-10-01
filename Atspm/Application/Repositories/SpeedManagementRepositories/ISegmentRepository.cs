using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface ISegmentRepository : IAsyncRepository<Segment>
    {
        public Task AddSegmentsAsync(IEnumerable<Segment> segments);
        public Task AddSegmentAsync(Segment segment);
        List<Segment> AllSegmentsWithEntity();
        Task<List<Segment>> GetSegmentsDetailsWithEntity(List<Guid> segmentIds);
        Task<List<Segment>> GetSegmentsDetail(List<Guid> segmentIds);
        List<Segment> AllSegmentsWithEntity(int source);
    }
}
