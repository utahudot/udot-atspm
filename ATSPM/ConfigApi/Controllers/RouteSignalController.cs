using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class RouteSignalController : AtspmConfigControllerBase<RouteSignal, int>
    {
        private readonly IRouteSignalsRepository _repository;

        public RouteSignalController(IRouteSignalsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
