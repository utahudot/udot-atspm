#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.Controllers/PurduePhaseTerminationControrller.cs
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
using Utah.Udot.Atspm.Business.PhaseTermination;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    /// <summary>
    /// Purdue phase termination report controller
    /// </summary>
    [ApiVersion(1.0)]
    public class PurduePhaseTerminationController : ReportControllerBase<PurduePhaseTerminationOptions, PhaseTerminationResult>
    {
        /// <inheritdoc/>
        public PurduePhaseTerminationController(IReportService<PurduePhaseTerminationOptions, PhaseTerminationResult> reportService, ILogger<PurduePhaseTerminationController> logger) : base(reportService, logger) { }
    }
}