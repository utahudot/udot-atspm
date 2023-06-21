using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class DetectionHardwareController : AtspmConfigControllerBase<DetectionHardware, int>
    {
        private readonly IDetectionHardwareRepository _repository;

        public DetectionHardwareController(IDetectionHardwareRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
