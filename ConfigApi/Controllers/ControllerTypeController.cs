using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class ControllerTypeController : AtspmConfigControllerBase<ControllerType, int>
    {
        private readonly IControllerTypeRepository _repository;

        public ControllerTypeController(IControllerTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
