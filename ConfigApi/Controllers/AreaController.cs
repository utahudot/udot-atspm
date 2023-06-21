using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class AreaController : AtspmConfigControllerBase<Area, int>
    {
        private readonly IAreaRepository _repository;

        public AreaController(IAreaRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
