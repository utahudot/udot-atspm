using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class RegionController : AtspmConfigControllerBase<Region, int>
    {
        private readonly IRegionsRepository _repository;

        public RegionController(IRegionsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
