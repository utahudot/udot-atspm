#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.Controllers/PreemptServiceController.cs
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
using Utah.Udot.Atspm.Business.PreemptService;

namespace Utah.Udot.Atspm.ReportApi.Controllers
{
    /// <summary>
    /// Preempt service report controller
    /// </summary>
    [ApiVersion(1.0)]
    public class PreemptServiceController : ReportControllerBase<PreemptServiceOptions, PreemptServiceResult>
    {
        /// <inheritdoc/>
        public PreemptServiceController(IReportService<PreemptServiceOptions, PreemptServiceResult> reportService, ILogger<PreemptServiceController> logger) : base(reportService, logger) { }
    }
}