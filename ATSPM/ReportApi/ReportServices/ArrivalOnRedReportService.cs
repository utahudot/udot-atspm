#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/ArrivalOnRedReportService.cs
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
using ATSPM.Application.Business;
using ATSPM.Application.Business.ArrivalOnRed;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Arrival on red report service
    /// </summary>
    public class ArrivalOnRedReportService : ReportServiceBase<ArrivalOnRedOptions, IEnumerable<ArrivalOnRedResult>>
    {
        private readonly ArrivalOnRedService arrivalOnRedService;
        private readonly LocationPhaseService LocationPhaseService;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;

        /// <inheritdoc/>
        public ArrivalOnRedReportService(
            ArrivalOnRedService arrivalOnRedService,
            LocationPhaseService LocationPhaseService,
            IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService
            )
        {
            this.arrivalOnRedService = arrivalOnRedService;
            this.LocationPhaseService = LocationPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ArrivalOnRedResult>> ExecuteAsync(ArrivalOnRedOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");

                return await Task.FromException<IEnumerable<ArrivalOnRedResult>>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");

                return await Task.FromException<IEnumerable<ArrivalOnRedResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<ArrivalOnRedResult>>();
            foreach (var phase in phaseDetails)
            {
                if ((phase.IsPermissivePhase && parameter.GetPermissivePhase) || !phase.IsPermissivePhase)
                {
                    tasks.Add(
                   GetChartDataByApproach(parameter, phase, controllerEventLogs, planEvents, Location.LocationDescription()));
                }

            }

            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PhaseNumber).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<ArrivalOnRedResult> GetChartDataByApproach(
            ArrivalOnRedOptions options,
            PhaseDetail phaseDetail,
            List<IndianaEvent> controllerEventLogs,
            List<IndianaEvent> planEvents,
            string LocationDescription)
        {
            var LocationPhase = await LocationPhaseService.GetLocationPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.BinSize,
                null,
                controllerEventLogs,
                planEvents,
                false
                );
            if (LocationPhase == null)
            {
                return null;
            }
            ArrivalOnRedResult viewModel = arrivalOnRedService.GetChartData(options, LocationPhase, phaseDetail.Approach);
            viewModel.LocationDescription = LocationDescription;
            return viewModel;
        }
    }
}
