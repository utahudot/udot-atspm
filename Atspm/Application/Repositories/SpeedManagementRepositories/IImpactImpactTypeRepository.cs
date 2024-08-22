using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IImpactImpactTypeRepository : IAsyncRepository<ImpactImpactType>
    {
        Task<IReadOnlyList<ImpactImpactType>> GetImpactTypesForImpactAsync(Guid impactId);
        Task RemoveAllImpactsFromImpactTypeAsync(Guid impactType);
        Task RemoveAllImpactTypesFromImpactIdAsync(Guid? impactId);
    }
}
