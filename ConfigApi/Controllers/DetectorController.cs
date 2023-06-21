using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class DetectorController : AtspmConfigControllerBase<Detector, int>
    {
        private readonly IDetectorRepository _repository;

        public DetectorController(IDetectorRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
