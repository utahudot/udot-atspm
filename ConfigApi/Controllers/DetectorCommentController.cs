using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class DetectorCommentController : AtspmConfigControllerBase<DetectorComment, int>
    {
        private readonly IDetectorCommentRepository _repository;

        public DetectorCommentController(IDetectorCommentRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
