using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.AppoachDelay;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class ApproachDelayReportService : ReportServiceBase<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>
    {
        private readonly ApproachDelayService _approachDelayService;
        private readonly LocationPhaseService _LocationPhaseService;
        private readonly ILocationRepository _LocationRepository;
        private readonly IControllerEventLogRepository _controllerEventLogRepository;
        private readonly PhaseService _phaseService;

        /// <inheritdoc/>
        public ApproachDelayReportService(
            ApproachDelayService approachDelayService,
            LocationPhaseService LocationPhaseService,
            ILocationRepository LocationRepository,
            IControllerEventLogRepository controllerEventLogRepository,
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
            var Location = _LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<ApproachDelayResult>>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = _controllerEventLogRepository.GetLocationEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<ApproachDelayResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
                parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = _phaseService.GetPhases(Location);
            var tasks = new List<Task<ApproachDelayResult>>();

            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataByApproach(parameter, phase, controllerEventLogs, planEvents, Location.LocationDescription()));
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

        protected async Task<ApproachDelayResult> GetChartDataByApproach(
            ApproachDelayOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
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
                false);
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
