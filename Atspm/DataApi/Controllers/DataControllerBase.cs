#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.ATSPM.DataApi.Controllers/DataControllerBase.cs
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

using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.ATSPM.DataApi.Controllers
{
    /// <summary>
    /// Provides a base implementation for API controllers that handle data operations.
    /// </summary>
    /// <remarks>
    /// This abstract base class defines common dependencies and validation logic used by
    /// derived controllers. It ensures consistent input validation for location identifiers
    /// and date ranges across multiple endpoints.
    /// </remarks>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class DataControllerBase(ILocationRepository locations, ILogger log) : ControllerBase
    {
        /// <summary>
        /// Repository used to validate and interact with location data.
        /// </summary>
        protected readonly ILocationRepository _locations = locations;

        /// <summary>
        /// Logger instance used for diagnostic and error logging within derived controllers.
        /// </summary>
        protected readonly ILogger _log = log;

        /// <summary>
        /// Validates common input parameters for data queries.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data is being requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the query range. Must be less than or equal to <paramref name="end"/>.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the query range. Must be greater than or equal to <paramref name="start"/>.
        /// </param>
        /// <returns>
        /// Returns <see cref="BadRequestObjectResult"/> if the date range is invalid,
        /// <see cref="NotFoundObjectResult"/> if the location does not exist,
        /// or <c>null</c> if validation succeeds.
        /// </returns>
        /// <remarks>
        /// This method is intended to be called at the beginning of controller actions
        /// to enforce uniform validation rules. Derived controllers can override this
        /// method to extend or customize validation behavior.
        /// </remarks>
        protected virtual async Task<ActionResult?> ValidateInputs(string locationIdentifier, DateTime start, DateTime end)
        {
            if (end <= start)
                return BadRequest(new ProblemDetails { Title = "Invalid date range", Detail = "End must be > start" });

            if (!await _locations.LocationExists(locationIdentifier))
                return NotFound(new ProblemDetails { Title = "Location not found", Detail = $"No location '{locationIdentifier}' exists." });

            return null; // success
        }
    }
}
