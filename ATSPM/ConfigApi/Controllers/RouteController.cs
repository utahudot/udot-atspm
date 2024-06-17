using Asp.Versioning;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Services;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Collections.Generic;
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
        public IActionResult CreateRouteWithLocations([FromBody] Data.Models.Route route)
        {
            if (route == null || route.RouteLocations == null)
            {
                return BadRequest();
            }

            try
            {
                _routeService.CreateRouteWithLocations(route, route.RouteLocations);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
