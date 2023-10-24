using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// ExternalLinks Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class ExternalLinkController : AtspmConfigControllerBase<ExternalLink, int>
    {
        private readonly IExternalLinksRepository _repository;

        /// <inheritdoc/>
        public ExternalLinkController(IExternalLinksRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
