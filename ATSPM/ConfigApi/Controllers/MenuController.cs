using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Menu Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MenuController : AtspmConfigControllerBase<MenuItem, int>
    {
        private readonly IMenuItemReposiotry _repository;

        /// <inheritdoc/>
        public MenuController(IMenuItemReposiotry repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
