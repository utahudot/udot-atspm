using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class LaneTypeController : AtspmConfigControllerBase<LaneType, int>
    {
        private readonly ILaneTypeRepository _repository;

        public LaneTypeController(ILaneTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
