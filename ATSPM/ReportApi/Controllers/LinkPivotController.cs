﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.Controllers/LinkPivotController.cs
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
using Utah.Udot.Atspm.Business.LinkPivot;
using Utah.Udot.Atspm.ReportApi.ReportServices;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    /// <summary>
    /// Left turn gap analysis report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class LinkPivotController : ReportControllerBase<LinkPivotOptions, LinkPivotResult>
    {
        private readonly LinkPivotReportService linkPivotReportService;
        /// <inheritdoc/>
        public LinkPivotController(IReportService<LinkPivotOptions, LinkPivotResult> reportService, LinkPivotReportService linkPivotReportService, ILogger<LinkPivotController> logger) : base(reportService, logger)
        {
            this.linkPivotReportService = linkPivotReportService;
        }

        [HttpPost("getPcdData")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LinkPivotPcdResult>> GetPcdData(LinkPivotPcdOptions options)
        {
            try
            {
                var result = await linkPivotReportService.GetPcdData(options);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}