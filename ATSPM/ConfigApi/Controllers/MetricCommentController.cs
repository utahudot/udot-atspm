using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class MetricCommentController : AtspmConfigControllerBase<MetricComment, int>
    {
        private readonly IMetricCommentRepository _repository;

        public MetricCommentController(IMetricCommentRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
