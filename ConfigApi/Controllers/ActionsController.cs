using ATSPM.Application.Repositories;

namespace ATSPM.ConfigApi.Controllers
{
    public class ActionsController : AtspmConfigControllerBase<Data.Models.Action, int>
    {
        private readonly IActionRepository _repository;

        public ActionsController(IActionRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
