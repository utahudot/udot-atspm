using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface IImpactImpactTypeRepository : IAsyncRepository<ImpactImpactType>
    {
        Task<IReadOnlyList<ImpactImpactType>> GetImpactTypesForImpactAsync(Guid impactId);
        Task RemoveAllImpactsFromImpactTypeAsync(Guid impactType);
        Task RemoveAllImpactTypesFromImpactIdAsync(Guid? impactId);
    }
}
