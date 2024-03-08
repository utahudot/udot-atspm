using Asp.Versioning;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Version history controller
    /// </summary>
    [ApiVersion(1.0)]
    public class VersionHistoryController : AtspmConfigControllerBase<VersionHistory, int>
    {
        private readonly IVersionHistoryRepository _repository;

        /// <inheritdoc/>
        public VersionHistoryController(IVersionHistoryRepository repository) : base(repository)
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
