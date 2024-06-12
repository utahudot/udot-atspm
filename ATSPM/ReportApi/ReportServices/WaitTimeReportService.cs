#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/WaitTimeReportService.cs
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
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.WaitTime;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Wait time report service
    /// </summary>
    public class WaitTimeReportService : ReportServiceBase<WaitTimeOptions, IEnumerable<WaitTimeResult>>
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly WaitTimeService waitTimeService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public WaitTimeReportService(
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            WaitTimeService waitTimeService,
            IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService
            )
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.waitTimeService = waitTimeService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<WaitTimeResult>> ExecuteAsync(WaitTimeOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<WaitTimeResult>>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<WaitTimeResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var eventCodes = new List<short>() {
                    82,
                    1,
                    44,
                    11,
                    43};
            var events = controllerEventLogs.GetEventsByEventCodes(parameter.Start, parameter.End, eventCodes);
            var terminationEvents = controllerEventLogs.Where(e =>
                new List<short> { 4, 5, 6, 7 }.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var splitsEventCodes = new List<short>();
            for (short i = 130; i <= 149; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = controllerEventLogs.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();

            var analysisPhaseDataCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                Location.LocationIdentifier,
            parameter.Start,
                parameter.End,
                planEvents,
                events,
                splitsEvents,
                null,
                terminationEvents,
                Location,
                1);
            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<WaitTimeResult>>();
            foreach (var phaseDetail in phaseDetails)
            {
                tasks.Add(waitTimeService.GetChartData(
                parameter,
                phaseDetail,
                events,
                analysisPhaseDataCollection.AnalysisPhases.Where(a => a.PhaseNumber == phaseDetail.PhaseNumber).First(),
                analysisPhaseDataCollection.Plans
                ));
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
    }
}
