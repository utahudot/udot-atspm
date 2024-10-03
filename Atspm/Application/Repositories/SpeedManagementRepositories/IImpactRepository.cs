using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IImpactRepository : IAsyncRepository<Impact>
    {
        Task<List<Impact>> GetImpactsForSegmentAsync(Guid segmentId);
        Task<Impact> GetInstanceDetails(Guid? impactId);
        Task<List<Impact>> GetInstancesDetails(List<Guid> impactIds);
        Task<Impact> UpdateImpactAsync(Impact item);
    }
}
