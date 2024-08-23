using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices
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