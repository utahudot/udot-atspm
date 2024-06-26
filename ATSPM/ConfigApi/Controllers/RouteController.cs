using Asp.Versioning;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Models;
using ATSPM.ConfigApi.Services;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Route controller
    /// </summary>
    [ApiVersion(1.0)]
    public class RouteController : AtspmConfigControllerBase<Data.Models.Route, int>
    {
        private readonly IRouteRepository _repository;
        private readonly IRouteService _routeService;

        /// <inheritdoc/>
        public RouteController(IRouteRepository repository, IRouteService routeService) : base(repository)
        {
            _repository = repository;
            _routeService = routeService;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="RouteLocation"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<RouteLocation>> GetRouteLocations([FromRoute] int key)
        {
            return Ok(_repository.GetList().Where(r => r.Id == key).SelectMany(r => r.RouteLocations));
        }

        #endregion

        #region Actions

        /// <summary>
        /// Creates a route with its associated route locations
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        [HttpPost("CreateRouteWithLocations")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult CreateRouteWithLocations([FromBody] RouteDto route)
        {
            if (route == null || route.RouteLocations == null)
            {
                return BadRequest();
            }

            try
            {
                _routeService.CreateOrUpdateRoute(route);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Creates a route with its associated route locations
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        [HttpPost("GetRouteWithExpandedLocations")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult GetRouteWithExpandedLocationsAsync([FromBody] RouteIdDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var routes = _routeService.GetRouteWithExpandedLocationsAsync(dto.RouteId);
                return Ok(routes);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
