using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class MeasuresDefaultController : AtspmConfigControllerBase<MeasuresDefault, int>
    {
        private readonly IMeasuresDefaultsRepository _repository;

        public MeasuresDefaultController(IMeasuresDefaultsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
