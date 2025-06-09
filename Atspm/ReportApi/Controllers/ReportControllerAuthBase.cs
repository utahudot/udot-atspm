#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.Controllers/ReportControllerAuthBase.cs
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

using AutoFixture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Infrastructure.LogMessages;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    /// <summary>
    /// Report controller base
    /// </summary>
    /// <typeparam name="Tin">Input options</typeparam>
    /// <typeparam name="Tout">Output results</typeparam>
    [ApiController]
    [Authorize()]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class ReportControllerAuthBase<Tin, Tout> : ControllerBase
    {
        private readonly IReportService<Tin, Tout> _reportService;
        private readonly ReportsLoggerLogMessages<Tin, Tout> _reportsLogMessages;

        /// <inheritdoc/>
        public ReportControllerAuthBase(IReportService<Tin, Tout> reportService, ILogger logger)
        {
            _reportService = reportService;
            _reportsLogMessages = new ReportsLoggerLogMessages<Tin, Tout>(logger, _reportService);
        }

        /// <summary>
        /// Get example data for testing
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual ActionResult<Tout> GetTestData()
        {
            return Ok(new Fixture().Create<Tout>());
        }

        /// <summary>
        /// Get report data
        /// </summary>
        /// <param name="options">Reporting options</param>
        /// <returns></returns>
        [HttpPost("getReportData")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<ActionResult<Tout>> GetReportData([FromBody] Tin options)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
                _reportsLogMessages.ReportStartedMessage(DateTime.Now, controllerName);
                var result = await _reportService.ExecuteAsync(options, null, HttpContext.RequestAborted);
                _reportsLogMessages.ReportCompletedMessage(DateTime.Now, controllerName);
                return Ok(result);
            }
            catch (Exception e)
            {
                _reportsLogMessages.ReportExecutionException(e);
                return BadRequest(e.Message);
            }
        }
    }
}
