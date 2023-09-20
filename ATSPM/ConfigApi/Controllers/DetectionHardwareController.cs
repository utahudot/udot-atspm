using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class DetectionHardwareController : AtspmConfigControllerBase<DetectionHardware, DetectionHardwareTypes>
    {
        private readonly IDetectionHardwareRepository _repository;

        public DetectionHardwareController(IDetectionHardwareRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
