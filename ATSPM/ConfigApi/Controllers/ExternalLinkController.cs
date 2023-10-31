using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// ExternalLinks Controller
    /// </summary>
    [ApiVersion(0.1)]
    public class ExternalLinkController : AtspmConfigControllerBase<ExternalLink, int>
    {
        private readonly IExternalLinksRepository _repository;

        /// <inheritdoc/>
        public ExternalLinkController(IExternalLinksRepository repository) : base(repository)
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
