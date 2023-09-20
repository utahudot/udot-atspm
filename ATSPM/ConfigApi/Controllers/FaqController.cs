using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class FaqController : AtspmConfigControllerBase<Faq, int>
    {
        private readonly IFaqRepository _repository;

        public FaqController(IFaqRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
