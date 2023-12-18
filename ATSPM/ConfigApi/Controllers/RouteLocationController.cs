using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Route Location controller
    /// </summary>
    [ApiVersion(1.0)]
    public class RouteLocationController : AtspmConfigControllerBase<RouteLocation, int>
    {
        private readonly IRouteLocationsRepository _repository;

        /// <inheritdoc/>
        public RouteLocationController(IRouteLocationsRepository repository) : base(repository)
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