#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/ArrivalOnRedReportService.cs
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

using Utah.Udot.Atspm.Business.PrioritySummary;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Priority Summary report service
    /// </summary>
    public class PrioritySummaryReportService : ReportServiceBase<PrioritySummaryOptions, PrioritySummaryResult>
    {
        private readonly PrioritySummaryService prioritySummaryService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;

        /// <inheritdoc/>
        public PrioritySummaryReportService(
            PrioritySummaryService prioritySummaryService,
            IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository)
        {
            this.prioritySummaryService = prioritySummaryService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
        }

        /// <inheritdoc/>
        public override async Task<PrioritySummaryResult> ExecuteAsync(PrioritySummaryOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);
            if (Location == null)
            {
                return await Task.FromException<PrioritySummaryResult>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(parameter.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                return await Task.FromException<PrioritySummaryResult>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            //Get all events 112-130
            var tspEventCodes = new List<short>() { 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130 };
            var events = controllerEventLogs.GetEventsByEventCodes(parameter.Start, parameter.End, tspEventCodes);
            PrioritySummaryResult result = prioritySummaryService.GetChartData(parameter, events);
            result.LocationDescription = Location.LocationDescription();

            return result;
        }
    }
}
