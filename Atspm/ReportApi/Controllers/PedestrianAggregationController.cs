#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.Controllers/PedestrianAggregationController.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Asp.Versioning;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.AggregationModels;
using Utah.Udot.Atspm.ReportApi.ReportServices;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    /// <summary>
    /// pedat report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PedestrianAggregationController : ControllerBase
    {
        private readonly PedestrianAggregationService _pedestrianAggregationService;

        /// <summary>
        /// Constructor with DI
        /// </summary>
        public PedestrianAggregationController(PedestrianAggregationService pedestrianAggregationService)
        {
            _pedestrianAggregationService = pedestrianAggregationService;
        }

        /// <summary>
        /// Get example data for testing
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual ActionResult<object> GetTestData()
        {
            return Ok(new Fixture().Create<object>());
        }

        /// <summary>
        /// Get traffic location data based on query
        /// </summary>
        /// <param name="query">Query parameters for location data</param>
        /// <returns>List of PedatLocationData objects</returns>
        [HttpPost("getReportData")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PedatLocationData>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<PedatLocationData>>> GetLocationData([FromBody] PedatLocationDataQuery query)
        {
            if (query == null || query.LocationIdentifiers == null || query.LocationIdentifiers.Count == 0)
                return BadRequest("Invalid query parameters.");

            var result = await _pedestrianAggregationService.ExecuteAsync(query, null);

            return Ok(result);
        }
    }
}
