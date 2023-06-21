using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class MovementTypeController : AtspmConfigControllerBase<MovementType, int>
    {
        private readonly IMovementTypeRepository _repository;

        public MovementTypeController(IMovementTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
