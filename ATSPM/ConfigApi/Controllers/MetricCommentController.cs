using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class MetricCommentController : AtspmConfigControllerBase<MeasureComment, int>
    {
        private readonly IMeasureCommentRepository _repository;

        public MetricCommentController(IMeasureCommentRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
