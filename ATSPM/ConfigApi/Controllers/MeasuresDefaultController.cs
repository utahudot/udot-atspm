using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// MeasureDefault Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MeasuresDefaultController : AtspmConfigControllerBase<MeasureOption, int>
    {
        private readonly IMeasureOptionsRepository _repository;

        /// <inheritdoc/>
        public MeasuresDefaultController(IMeasureOptionsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
