using ATSPM.Application.Repositories;

namespace ATSPM.ConfigApi.Controllers
{
    public class RouteController : AtspmConfigControllerBase<Data.Models.Route, int>
    {
        private readonly IRouteRepository _repository;

        public RouteController(IRouteRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
