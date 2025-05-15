#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/RouteController.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Utah.Udot.Atspm.ConfigApi.Services;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.ConfigApi.DTO;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Route controller
    /// </summary>
    [ApiVersion(1.0)]
    public class RouteController : GeneralPolicyControllerBase<Data.Models.Route, int>
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
        [HttpPost("api/v1/UpsertRoute")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult UpsertRoute([FromBody] RouteDto route)
        {
            if (route == null || route.RouteLocations == null)
            {
                return BadRequest();
            }

            try
            {
                var routeResult = _routeService.UpsertRoute(route);
                return Ok(routeResult);
            }
            catch (Exception ex)
            {
                return StatusCode(Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("api/v1/GetRouteView/{id}")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult GetRouteView(int id, bool includeLocationDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var routes = _routeService.GetRouteWithExpandedLocations(id, includeLocationDetail);
                return Ok(routes);
            }
            catch (Exception ex)
            {
                return StatusCode(Status500InternalServerError, ex.Message);
            }
        }


        #endregion
    }
}
