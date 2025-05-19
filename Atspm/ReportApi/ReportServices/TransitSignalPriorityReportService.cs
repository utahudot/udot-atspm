#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/TransitSignalPriorityReportService.cs
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

using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Business.TransitSignalPriorityRequest;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class TransitSignalPriorityReportService : ReportServiceBase<TransitSignalPriorityOptions, List<TransitSignalPriorityResult>>
    {
        private readonly TransitSignalPriorityService _transitSignalPriorityService;

        /// <inheritdoc/>
        public TransitSignalPriorityReportService(
            TransitSignalPriorityService transitSignalPriorityService
            )
        {
            _transitSignalPriorityService = transitSignalPriorityService;
        }

        /// <inheritdoc/>
        public override async Task<List<TransitSignalPriorityResult>> ExecuteAsync(TransitSignalPriorityOptions parameters, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            return await _transitSignalPriorityService.GetChartDataAsync(parameters);
        }

    }
}
