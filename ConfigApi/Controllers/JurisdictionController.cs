using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class JurisdictionController : AtspmConfigControllerBase<Jurisdiction, int>
    {
        private readonly IJurisdictionRepository _repository;

        public JurisdictionController(IJurisdictionRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
