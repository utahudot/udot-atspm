#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/ApproachSpeedReportService.cs
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
using ATSPM.Application.Business.ApproachSpeed;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Approach speed report service
    /// </summary>
    public class ApproachSpeedReportService : ReportServiceBase<ApproachSpeedOptions, IEnumerable<ApproachSpeedResult>>
    {
        private readonly ApproachSpeedService approachSpeedService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly IApproachRepository approachRepository;
        private readonly ISpeedEventLogRepository speedEventRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public ApproachSpeedReportService(
            ApproachSpeedService approachSpeedService,
            IIndianaEventLogRepository controllerEventLogRepository,
            IApproachRepository approachRepository,
            ISpeedEventLogRepository speedEventRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService)
        {
            this.approachSpeedService = approachSpeedService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachRepository = approachRepository;
            this.speedEventRepository = speedEventRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ApproachSpeedResult>> ExecuteAsync(ApproachSpeedOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);
            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No data found");
                return await Task.FromException<IEnumerable<ApproachSpeedResult>>(new NullReferenceException("No Controller Event Logs found for this signal on this date"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<ApproachSpeedResult>>();

            foreach (var phaseDetail in phaseDetails)
            {
                tasks.Add(GetChartDataByApproach(parameter, controllerEventLogs, planEvents, phaseDetail, Location.LocationDescription()));
            }
            var results = await Task.WhenAll(tasks);
            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PhaseNumber).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No data found");
            //}

            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<ApproachSpeedResult> GetChartDataByApproach(
            ApproachSpeedOptions options,
            List<IndianaEvent> controllerEventLogs,
            List<IndianaEvent> planEvents,
            PhaseDetail phaseDetail,
            string LocationDescription)
        {
            var detectors = phaseDetail.Approach.GetDetectorsForMetricType(options.MetricTypeId);
            Detector detector;
            if (detectors.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                detector = detectors.First();
            }
            var speedEvents = speedEventRepository.GetSpeedEventsByDetector(phaseDetail.Approach.Location.LocationIdentifier,
                detector,
                options.Start,
                options.End,
                detector.MinSpeedFilter ?? 5).ToList();
            if (speedEvents.IsNullOrEmpty())
            {
                return null;
            }
            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                phaseDetail.PhaseNumber,
                phaseDetail.UseOverlap,
                options.Start,
                options.End);
            ApproachSpeedResult viewModel = approachSpeedService.GetChartData(
                options,
                cycleEvents.ToList(),
                planEvents,
                speedEvents,
                detector);
            viewModel.LocationDescription = LocationDescription;
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
