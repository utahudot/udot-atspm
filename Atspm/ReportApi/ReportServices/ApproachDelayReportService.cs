#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/ApproachDelayReportService.cs
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

using Microsoft.IdentityModel.Tokens;
using Utah.Udot.Atspm.Business.AppoachDelay;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class ApproachDelayReportService : ReportServiceBase<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>
    {
        private readonly ApproachDelayService _approachDelayService;
        private readonly LocationPhaseService _LocationPhaseService;
        private readonly ILocationRepository _LocationRepository;
        private readonly IIndianaEventLogRepository _controllerEventLogRepository;
        private readonly PhaseService _phaseService;

        /// <inheritdoc/>
        public ApproachDelayReportService(
            ApproachDelayService approachDelayService,
            LocationPhaseService LocationPhaseService,
            ILocationRepository LocationRepository,
            IIndianaEventLogRepository controllerEventLogRepository,
            PhaseService phaseService
            )
        {
            _approachDelayService = approachDelayService;
            _LocationPhaseService = LocationPhaseService;
            _LocationRepository = LocationRepository;
            _controllerEventLogRepository = controllerEventLogRepository;
            _phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ApproachDelayResult>> ExecuteAsync(ApproachDelayOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = _LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<ApproachDelayResult>>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = _controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier,
                parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                await Task.FromException<IEnumerable<Analysis.ApproachDelay.ApproachDelayResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
                parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = _phaseService.GetPhases(Location);
            var tasks = new List<Task<ApproachDelayResult>>();

            foreach (var phase in phaseDetails)
            {
                if (phase.IsPermissivePhase && parameter.GetPermissivePhase || !phase.IsPermissivePhase)
                {
                    tasks.Add(GetChartDataByApproach(parameter, phase, controllerEventLogs, planEvents, Location.LocationDescription()));
                }
            }

            var results = await Task.WhenAll(tasks);
            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PhaseNumber).ToList();

            return finalResultcheck;
        }

        protected async Task<ApproachDelayResult> GetChartDataByApproach(
            ApproachDelayOptions options,
            PhaseDetail phaseDetail,
            List<IndianaEvent> controllerEventLogs,
            List<IndianaEvent> planEvents,
            string LocationDescription)
        {
            var LocationPhase = await _LocationPhaseService.GetLocationPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.BinSize,
                null,
                controllerEventLogs,
                planEvents,
                options.GetVolume);
            if (LocationPhase == null)
            {
                return null;
            }
            ApproachDelayResult viewModel = _approachDelayService.GetChartData(
                options,
                phaseDetail,
                LocationPhase);
            viewModel.LocationDescription = LocationDescription;
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
