using ATSPM.Application.Reports.Business.ApproachSpeed;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using ATSPM.Infrastructure.Repositories;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachSpeedController : ControllerBase
    {
        private readonly ApproachSpeedService approachSpeedService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly IApproachRepository approachRepository;
        private readonly ISpeedEventRepository speedEventRepository;

        public ApproachSpeedController(
            ApproachSpeedService approachSpeedService,
            IControllerEventLogRepository controllerEventLogRepository,
            IApproachRepository approachRepository,
            ISpeedEventRepository speedEventRepository)
        {
            this.approachSpeedService = approachSpeedService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachRepository = approachRepository;
            this.speedEventRepository = speedEventRepository;
        }

        [HttpGet("test")]
        public ApproachSpeedResult Test()
        {
            Fixture fixture = new();
            ApproachSpeedResult viewModel = fixture.Create<ApproachSpeedResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public ApproachSpeedResult GetChartData([FromBody] ApproachSpeedOptions options)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            var detector = approach.GetDetectorsForMetricType(options.MetricTypeId).First();
            var speedEvents = speedEventRepository.GetSpeedEventsByDetector(
                detector,
                options.Start,
                options.End,
                detector.MinSpeedFilter ?? 5).ToList();
            var events = GetCycleEvents(options.UsePermissivePhase, options.Start, options.End, approach);
            events.AddRange(controllerEventLogRepository.GetSignalEventsByEventCode(approach.SignalId, options.Start, options.End, 131).ToList());
            ApproachSpeedResult viewModel = approachSpeedService.GetChartData(options);
            return viewModel;
        }

        private List<ControllerEventLog> GetCycleEvents(bool getPermissivePhase, DateTime startDate,
           DateTime endDate, Approach approach)
        {
            List<ControllerEventLog> cycleEvents;
            if (getPermissivePhase)
            {
                var cycleEventNumbers = approach.IsPermissivePhaseOverlap
                    ? new List<int> { 61, 63, 64, 66 }
                    : new List<int> { 1, 8, 9 };
                cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
                    endDate, cycleEventNumbers, approach.PermissivePhaseNumber.Value).ToList();
            }
            else
            {
                var cycleEventNumbers = approach.IsProtectedPhaseOverlap
                    ? new List<int> { 61, 63, 64, 66 }
                    : new List<int> { 1, 8, 9 };
                cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
                    endDate, cycleEventNumbers, approach.ProtectedPhaseNumber).ToList();
            }

            return cycleEvents;
        }
    }
}
