using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.TurningMovementCounts;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurningMovementCountsController : ControllerBase
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly TurningMovementCountsService turningMovementCountsService;
        private readonly ISignalRepository signalRepository;
        private readonly PlanService planService;

        public TurningMovementCountsController(
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

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public TurningMovementCountsResult Test()
        {
            Fixture fixture = new();
            TurningMovementCountsResult viewModel = fixture.Create<TurningMovementCountsResult>();
            return viewModel;
        }


        [HttpPost("getChartData")]
        public async Task<IEnumerable<TurningMovementCountsResult>> GetChartData([FromBody] TurningMovementCountsOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var plans = planService.GetBasicPlans(options.Start, options.End, options.SignalIdentifier, planEvents);

            var tasks = new List<Task<IEnumerable<TurningMovementCountsResult>>>();
            foreach (var laneType in Enum.GetValues(typeof(LaneTypes)))
            {
                tasks.Add(
                    GetChartDataForLaneType(
                    signal,
                    (LaneTypes)laneType,
                    options,
                    controllerEventLogs,
                    plans.ToList())
                    );
            }
            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null).SelectMany(r => r);
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
                        d.MovementTypeId == MovementTypes.T
                        || d.MovementTypeId == MovementTypes.TL
                        || d.MovementTypeId == MovementTypes.TR).ToList();
                    }
                    else
                    {
                        movementTypeDetectors = detectorsForDirection.Where(d => d.MovementTypeId == movementType).ToList();
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
                    detector.DetChannel,
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
