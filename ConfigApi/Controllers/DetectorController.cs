using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class DetectorController : AtspmConfigControllerBase<Detector, int>
    {
        private readonly IDetectorRepository _repository;

        public DetectorController(IDetectorRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
