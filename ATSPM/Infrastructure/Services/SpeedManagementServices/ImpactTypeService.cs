using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
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

        public async Task UpsertAsync(ImpactType impactType)
        {
            await impactTypeRepository.UpdateAsync(impactType);
        }

        public async Task DeleteAsync(ImpactType existingImpactType)
        {
            await impactTypeRepository.RemoveAsync(existingImpactType);
        }

        public async Task<ImpactType> GetImpactTypeById(int id)
        {
            return await impactTypeRepository.LookupAsync(id);
        }

        public async Task<IReadOnlyList<ImpactType>> ListImpactTypes()
        {
            return await impactTypeRepository.GetListImpactTypeAsync();
        }
    }
}