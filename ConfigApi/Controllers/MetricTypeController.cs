using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class MetricTypeController : AtspmConfigControllerBase<MetricType, int>
    {
        private readonly IMetricTypeRepository _repository;

        public MetricTypeController(IMetricTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
