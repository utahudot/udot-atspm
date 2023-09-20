using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class LaneTypeController : AtspmConfigControllerBase<LaneType, LaneTypes>
    {
        private readonly ILaneTypeRepository _repository;

        public LaneTypeController(ILaneTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
