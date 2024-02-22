using ATSPM.Application.Business;
using ATSPM.Application.Business.ApproachSpeed;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Approach speed report service
    /// </summary>
    public class ApproachSpeedReportService : ReportServiceBase<ApproachSpeedOptions, IEnumerable<ApproachSpeedResult>>
    {
        private readonly ApproachSpeedService approachSpeedService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly IApproachRepository approachRepository;
        private readonly ISpeedEventRepository speedEventRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public ApproachSpeedReportService(
            ApproachSpeedService approachSpeedService,
            IControllerEventLogRepository controllerEventLogRepository,
            IApproachRepository approachRepository,
            ISpeedEventRepository speedEventRepository,
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
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);
            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

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
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
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
            var speedEvents = speedEventRepository.GetSpeedEventsByDetector(
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
