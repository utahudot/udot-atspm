using Asp.Versioning;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Menu items controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MenuItemsController : AtspmConfigControllerBase<MenuItem, int>
    {
        private readonly IMenuItemReposiotry _repository;

        /// <inheritdoc/>
        public MenuItemsController(IMenuItemReposiotry repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        #endregion

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}
