using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class ImpactTypeService
    {
        private readonly IImpactTypeRepository impactTypeRepository;

        public ImpactTypeService(IImpactTypeRepository _impactTypeRepository)
        {
            impactTypeRepository = _impactTypeRepository;
        }

        public async Task<ImpactType> UpsertAsync(ImpactType impactType)
        {
            return await impactTypeRepository.UpsertImpactType(impactType);
        }

        public async Task DeleteAsync(ImpactType existingImpactType)
        {
            await impactTypeRepository.RemoveAsync(existingImpactType);
        }

        public async Task<ImpactType> GetImpactTypeById(Guid id)
        {
            return await impactTypeRepository.LookupAsync(id);
        }

        public async Task<IReadOnlyList<ImpactType>> ListImpactTypes()
        {
            return await impactTypeRepository.GetListImpactTypeAsync();
        }
    }
}