using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Route signal controller
    /// </summary>
    [ApiVersion(1.0)]
    public class RouteSignalController : AtspmConfigControllerBase<RouteSignal, int>
    {
        private readonly IRouteSignalsRepository _repository;

        /// <inheritdoc/>
        public RouteSignalController(IRouteSignalsRepository repository) : base(repository)
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