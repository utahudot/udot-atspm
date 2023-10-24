using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Menu Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MenuController : AtspmConfigControllerBase<Menu, int>
    {
        private readonly IMenuRepository _repository;

        /// <inheritdoc/>
        public MenuController(IMenuRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
