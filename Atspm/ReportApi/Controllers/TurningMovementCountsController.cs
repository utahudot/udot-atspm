﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.Controllers/TurningMovementCountsController.cs
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
using Utah.Udot.Atspm.Business.TurningMovementCounts;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    /// <summary>
    /// Turning movement count report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class TurningMovementCountsController : ReportControllerBase<TurningMovementCountsOptions, TurningMovementCountsResult>
    {
        /// <inheritdoc/>
        public TurningMovementCountsController(IReportService<TurningMovementCountsOptions, TurningMovementCountsResult> reportService, ILogger<TurningMovementCountsController> logger) : base(reportService, logger) { }
    }
}