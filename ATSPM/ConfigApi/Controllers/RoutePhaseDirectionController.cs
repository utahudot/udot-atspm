using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class RoutePhaseDirectionController : AtspmConfigControllerBase<RoutePhaseDirection, int>
    {
        private readonly IRoutePhaseDirectionRepository _repository;

        public RoutePhaseDirectionController(IRoutePhaseDirectionRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
