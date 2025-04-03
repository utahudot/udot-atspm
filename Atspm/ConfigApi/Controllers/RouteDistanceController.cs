#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/RouteDistanceController.cs
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
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// RouteDistance controller
    /// </summary>
    [ApiVersion(1.0)]
    public class RouteDistanceController : GeneralPolicyControllerBase<RouteDistance, int>
    {
        private readonly IRouteDistanceRepository _repository;

        /// <inheritdoc/>
        public RouteDistanceController(IRouteDistanceRepository repository) : base(repository)
        {
            _repository = repository;
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
        public ActionResult<IEnumerable<RouteLocation>> GetPreviousLocations([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<RouteLocation>>(key);
        }

        /// <summary>
        /// <see cref="RouteLocation"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<RouteLocation>> GetNextLocations([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<RouteLocation>>(key);
        }

        #endregion

        #region Actions

        #endregion

        #region Functions

        /// <summary>
        /// Gets the <see cref="RouteDistance"/> that contains <paramref name="locationA"/> and <paramref name="locationB"/> 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Select)]
        [ProducesResponseType(typeof(RouteDistance), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        public IActionResult GetRouteDistanceByLocationIdentifiers(string locationA, string locationB)
        {
            return Ok(_repository.GetRouteDistanceByLocationIdentifiers(locationA, locationB));
        }

        #endregion
    }
}
