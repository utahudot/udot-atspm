﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.Controllers/TimeSpaceDiagramAverageController.cs
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
using Utah.Udot.Atspm.Business.TimeSpaceDiagram;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class TimeSpaceDiagramAverageController : ReportControllerBase<TimeSpaceDiagramAverageOptions, IEnumerable<TimeSpaceDiagramAverageResult>>
    {
        /// <inheritdoc/>
        public TimeSpaceDiagramAverageController(IReportService<TimeSpaceDiagramAverageOptions, IEnumerable<TimeSpaceDiagramAverageResult>> reportService, ILogger<TimeSpaceDiagramAverageController> logger) : base(reportService, logger) { }
    }
}
