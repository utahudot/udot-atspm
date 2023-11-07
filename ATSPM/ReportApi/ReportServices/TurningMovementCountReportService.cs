using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.TurningMovementCounts;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Turning movement count report service
    /// </summary>
    public class TurningMovementCountReportService : ReportServiceBase<TurningMovementCountsOptions, IEnumerable<TurningMovementCountsResult>>
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly TurningMovementCountsService turningMovementCountsService;
        private readonly ISignalRepository signalRepository;
        private readonly PlanService planService;

        public TurningMovementCountReportService(
            IControllerEventLogRepository controllerEventLogRepository,
            TurningMovementCountsService turningMovementCountsService,
            ISignalRepository signalRepository,
            PlanService planService
            )
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.turningMovementCountsService = turningMovementCountsService;
            this.signalRepository = signalRepository;
            this.planService = planService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<TurningMovementCountsResult>> ExecuteAsync(TurningMovementCountsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);

            if (signal == null)
            {
                //return BadRequest("Signal not found");
                return await Task.FromException<IEnumerable<TurningMovementCountsResult>>(new NullReferenceException("Signal not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<TurningMovementCountsResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var plans = planService.GetBasicPlans(parameter.Start, parameter.End, parameter.SignalIdentifier, planEvents);
            var tasks = new List<Task<IEnumerable<TurningMovementCountsResult>>>();
            foreach (var laneType in Enum.GetValues(typeof(LaneTypes)))
            {
                tasks.Add(
                    GetChartDataForLaneType(
                    signal,
                (LaneTypes)laneType,
                    parameter,
                    controllerEventLogs,
                    plans.ToList())
                    );
            }
            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).SelectMany(r => r).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<IEnumerable<TurningMovementCountsResult>> GetChartDataForLaneType(
            Signal signal,
            LaneTypes laneType,
            TurningMovementCountsOptions options,
            List<ControllerEventLog> controllerEventLogs,
            List<Plan> plans)
        {
            var directions = signal.Approaches.Select(a => a.DirectionTypeId).Distinct().ToList();
            var tasks = new List<Task<TurningMovementCountsResult>>();
            foreach (var direction in directions)
            {
                var detectorsForDirection = signal.Approaches.Where(a => a.DirectionTypeId == direction).SelectMany(a => a.GetDetectorsForMetricType(options.MetricTypeId)).ToList();

                var movementTypesSorted = new List<MovementTypes> { MovementTypes.L, MovementTypes.T, MovementTypes.R };
                foreach (var movementType in movementTypesSorted)
                {
                    var movementTypeDetectors = new List<Detector>();
                    if (movementType == MovementTypes.T)
                    {
                        movementTypeDetectors = detectorsForDirection.Where(d =>
                        d.MovementType == MovementTypes.T
                        || d.MovementType == MovementTypes.TL
                        || d.MovementType == MovementTypes.TR).ToList();
                    }
                    else
                    {
                        movementTypeDetectors = detectorsForDirection.Where(d => d.MovementType == movementType).ToList();
                    }
                    if (!movementTypeDetectors.IsNullOrEmpty())
                    {
                        tasks.Add(GetChartDataByMovementType(
                            options,
                            plans,
                            controllerEventLogs,
                            movementTypeDetectors,
                            movementType,
                            laneType,
                            signal.SignalIdentifier,
                            signal.SignalDescription(),
                            direction));
                    }
                }
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<TurningMovementCountsResult> GetChartDataByMovementType(
            TurningMovementCountsOptions options,
            List<Plan> planEvents,
            List<ControllerEventLog> controllerEventLogs,
            List<Detector> detectors,
            MovementTypes movementType,
            LaneTypes laneType,
            string signalIdentifier,
            string signalDescription,
            DirectionTypes directionType)
        {
            var detectorEvents = new List<ControllerEventLog>();
            foreach (var detector in detectors)
            {
                detectorEvents.AddRange(controllerEventLogs.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
                    options.Start,
                    options.End,
                    new List<int>() { 82 },
                    detector.DetectorChannel,
                    detector.GetOffset(),
                    detector.LatencyCorrection).ToList());
            }
            var result = turningMovementCountsService.GetChartData(
                detectors,
                laneType,
                movementType,
                directionType,
                options,
                detectorEvents,
                planEvents,
                signalIdentifier,
                signalDescription);

            return await result;
        }
    }
}
