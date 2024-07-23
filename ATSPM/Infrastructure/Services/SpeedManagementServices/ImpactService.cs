using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;

namespace ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class ImpactService
    {
        private readonly IImpactRepository _impactRepository;

        public ImpactService(IImpactRepository impactRepository)
        {
            _impactRepository = impactRepository;
        }

    }
}