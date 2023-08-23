using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class MovementTypeController : AtspmConfigControllerBase<MovementType, MovementTypes>
    {
        private readonly IMovementTypeRepository _repository;

        public MovementTypeController(IMovementTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
