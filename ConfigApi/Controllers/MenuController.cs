using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class MenuController : AtspmConfigControllerBase<Menu, int>
    {
        private readonly IMenuRepository _repository;

        public MenuController(IMenuRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
