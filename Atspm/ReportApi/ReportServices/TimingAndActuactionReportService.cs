#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/TimingAndActuactionReportService.cs
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

using System.ComponentModel.DataAnnotations;
using Utah.Udot.Atspm.Business.TimingAndActuation;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Timing and actuation report service
    /// </summary>
    public class TimingAndActuactionReportService : ReportServiceBase<TimingAndActuationsOptions, IEnumerable<TimingAndActuationsForPhaseResult>>
    {
        private readonly TimingAndActuationsForPhaseService timingAndActuationsForPhaseService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public TimingAndActuactionReportService(
            TimingAndActuationsForPhaseService timingAndActuationsForPhaseService,
            IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService
            )
        {
            this.timingAndActuationsForPhaseService = timingAndActuationsForPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<TimingAndActuationsForPhaseResult>> ExecuteAsync(TimingAndActuationsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<TimingAndActuationsForPhaseResult>>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<TimingAndActuationsForPhaseResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<TimingAndActuationsForPhaseResult>>();

            foreach (var phase in phaseDetails)
            {
                var eventCodes = new List<short> { };
                var globalEventCodes = parameter.GlobalEventCodesList?.Any() != true ? new List<short> { 81, 82, 89, 90 } : parameter.GlobalEventCodesList;
                eventCodes.AddRange(globalEventCodes);
                eventCodes.AddRange(timingAndActuationsForPhaseService.GetPedestrianIntervalEventCodes(phase.Approach.IsPedestrianPhaseOverlap));
                if (parameter.PhaseEventCodesList != null)
                    eventCodes.AddRange(parameter.PhaseEventCodesList);
                tasks.Add(GetChartDataForPhase(parameter, controllerEventLogs, phase, eventCodes, phase.IsPermissivePhase));
            }
            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PhaseNumberSort).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<TimingAndActuationsForPhaseResult> GetChartDataForPhase(
            TimingAndActuationsOptions options,
            List<IndianaEvent> controllerEventLogs,
            PhaseDetail phaseDetail,
            List<short> eventCodes,
            bool usePermissivePhase)
        {
            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(phaseDetail.UseOverlap));
            var approachevents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddMinutes(-15),
                options.End.AddMinutes(15),
                eventCodes).ToList();
            var viewModel = timingAndActuationsForPhaseService.GetChartData(options, phaseDetail, approachevents, usePermissivePhase);
            viewModel.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            string approachDescription = GetApproachDescription(phaseDetail);
            viewModel.ApproachDescription = approachDescription;
            return viewModel;
        }

        private static string GetApproachDescription(PhaseDetail phaseDetail)
        {
            DirectionTypes direction = phaseDetail.Approach.DirectionTypeId;
            string directionTypeName = direction.GetAttributeOfType<DisplayAttribute>().Name;
            var ignoreDetectionTypes = new List<DetectionTypes> { DetectionTypes.AC, DetectionTypes.AS, DetectionTypes.AP };
            var filteredDetectors = phaseDetail.Approach.Detectors.Where(d => d.DetectionTypes.Any(t => !ignoreDetectionTypes.Contains(t.Id)));
            string approachDescription = "";
            if (filteredDetectors.Any())
            {
                MovementTypes movementType = filteredDetectors.ToList()[0].MovementType;
                string movementTypeName = movementType.GetAttributeOfType<DisplayAttribute>().Name;
                approachDescription = $"{directionTypeName} {movementTypeName} Ph{phaseDetail.PhaseNumber}";
            }
            else
            {
                approachDescription = $"{directionTypeName} Ph{phaseDetail.PhaseNumber}";
            }
            return approachDescription;
        }
    }
}
