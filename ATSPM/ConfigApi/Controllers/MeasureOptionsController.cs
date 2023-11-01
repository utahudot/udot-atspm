using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Measure options Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MeasureOptionsController : AtspmConfigControllerBase<MeasureOption, int>
    {
        private readonly IMeasureOptionsRepository _repository;

        /// <inheritdoc/>
        public MeasureOptionsController(IMeasureOptionsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
