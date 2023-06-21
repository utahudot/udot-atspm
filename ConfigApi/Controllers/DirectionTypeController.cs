using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class DirectionTypeController : AtspmConfigControllerBase<DirectionType, int>
    {
        private readonly IDirectionTypeRepository _repository;

        public DirectionTypeController(IDirectionTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
