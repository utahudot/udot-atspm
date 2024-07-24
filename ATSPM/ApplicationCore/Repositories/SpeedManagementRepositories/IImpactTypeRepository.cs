using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface IImpactTypeRepository : IAsyncRepository<ImpactType>
    {
        Task<IReadOnlyList<ImpactType>> GetListImpactTypeAsync();
    }
}
