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

using Utah.Udot.Atspm.Business.PriorityDetails;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Priority Details report service
    /// </summary>
    public class PriorityDetailsReportService : ReportServiceBase<PriorityDetailsOptions, IEnumerable<PriorityDetailsResult>>
    {
        private readonly PriorityDetailsService priorityDetailsService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;
        private readonly CycleService cycleService;

        /// <inheritdoc/>
        public PriorityDetailsReportService(
            PriorityDetailsService priorityDetailsService,
            IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService,
            CycleService cycleService)
        {
            this.priorityDetailsService = priorityDetailsService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.cycleService = cycleService;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<PriorityDetailsResult>> ExecuteAsync(PriorityDetailsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);
            if (Location == null)
            {
                return await Task.FromException<IEnumerable<PriorityDetailsResult>>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(parameter.LocationIdentifier, parameter.Start.AddHours(-1), parameter.End.AddHours(1)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                return await Task.FromException<IEnumerable<PriorityDetailsResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }
            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<PriorityDetailsResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForPhase(parameter, controllerEventLogs, phase, phase.IsPermissivePhase));
            }
            var results = await Task.WhenAll(tasks);
            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PhaseNumberSort).ToList();
            return finalResultcheck;
        }

        public async Task<PriorityDetailsResult> GetChartDataForPhase(
            PriorityDetailsOptions options,
            List<IndianaEvent> controllerEventLogs,
            PhaseDetail phaseDetail,
            bool usePermissivePhase)
        {
            var cycleEvents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddMinutes(-15),
                options.End.AddMinutes(15),
                cycleService.GetCycleCodes(phaseDetail.UseOverlap))
                .Where(e => e.EventParam == phaseDetail.PhaseNumber).ToList();

            var tspEventCodes = new List<short>() { 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130 };
            var priorityEvents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddMinutes(-15),
                options.End.AddMinutes(15),
                tspEventCodes)
                //.ToList();
                .Where(e => e.EventParam == phaseDetail.Approach.TransitSignalPriorityNumber).ToList();

            var detectionEvents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddMinutes(-15),
                options.End.AddMinutes(15),
                new List<short>() { 81, 82 }).ToList();

            var viewModel = priorityDetailsService.GetChartData(options, phaseDetail, cycleEvents, priorityEvents, detectionEvents, usePermissivePhase);
            viewModel.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            string approachDescription = phaseDetail.GetApproachDescription();
            viewModel.ApproachDescription = approachDescription;

            return viewModel;
        }

    }
}
