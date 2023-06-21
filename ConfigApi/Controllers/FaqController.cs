using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class FaqController : AtspmConfigControllerBase<Faq, int>
    {
        private readonly IFaqRepository _repository;

        public FaqController(IFaqRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
