using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class DetectionTypeController : AtspmConfigControllerBase<DetectionType, DetectionTypes>
    {
        private readonly IDetectionTypeRepository _repository;

        public DetectionTypeController(IDetectionTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
