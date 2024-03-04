using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// FAQ Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class FaqController : AtspmGeneralConfigBase<Faq, int>
    {
        private readonly IFaqRepository _repository;

        /// <inheritdoc/>
        public FaqController(IFaqRepository repository) : base(repository)
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
