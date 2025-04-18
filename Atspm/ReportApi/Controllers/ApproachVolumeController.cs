﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.Controllers/ApproachVolumeController.cs
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
using Utah.Udot.Atspm.Business.ApproachVolume;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    /// <summary>
    /// Approach volume report controller
    /// </summary>
    [ApiVersion(1.0)]
    public class ApproachVolumeController : ReportControllerBase<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>>
    {
        /// <inheritdoc/>
        public ApproachVolumeController(IReportService<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>> reportService, ILogger<ApproachVolumeController> logger) : base(reportService, logger) { }
    }
}