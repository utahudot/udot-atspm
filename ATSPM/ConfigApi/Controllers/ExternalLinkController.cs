using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class ExternalLinkController : AtspmConfigControllerBase<ExternalLink, int>
    {
        private readonly IExternalLinksRepository _repository;

        public ExternalLinkController(IExternalLinksRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
