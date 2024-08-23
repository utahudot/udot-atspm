using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IImpactTypeRepository : IAsyncRepository<ImpactType>
    {
        Task<IReadOnlyList<ImpactType>> GetListImpactTypeAsync();
        Task<ImpactType> UpsertImpactType(ImpactType item);
    }
}
