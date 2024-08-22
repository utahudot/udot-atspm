using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IImpactRepository : IAsyncRepository<Impact>
    {
        Task<List<Impact>> GetInstancesDetails(List<Guid> impactIds);
        Task<Impact> UpdateImpactAsync(Impact item);
    }
}
