﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.Controllers/ApproachDelayController.cs
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
using Utah.Udot.Atspm.Business.AppoachDelay;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    /// <summary>
    /// Approach delay report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class ApproachDelayController : ReportControllerBase<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>
    {
        /// <inheritdoc/>
        public ApproachDelayController(IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>> reportService, ILogger<ApproachDelayController> logger) : base(reportService, logger) { }
    }
}