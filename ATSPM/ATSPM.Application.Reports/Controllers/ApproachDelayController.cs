using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Data.Models;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachDelayController : ControllerBase
    {
        private readonly ApproachDelayService approachDelayService;
        private readonly SignalPhaseService signalPhaseService;
        private readonly IApproachRepository approachRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public ApproachDelayController(
            ApproachDelayService approachDelayService,
            SignalPhaseService signalPhaseService,
            IApproachRepository approachRepository,
            IControllerEventLogRepository controllerEventLogRepository
            )
        {
            this.approachDelayService = approachDelayService;
            this.signalPhaseService = signalPhaseService;
            this.approachRepository = approachRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        [HttpGet("test")]
        public ApproachDelayResult Test()
        {
            Fixture fixture = new();
            ApproachDelayResult viewModel = fixture.Create<ApproachDelayResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public ApproachDelayResult GetChartData([FromBody] ApproachDelayOptions options)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            var approachDetectors = approach.GetDetectorsForMetricType(options.MetricTypeId).ToList();
            var cycleEventCodes = options.GetPermissivePhase ? new List<int> { 61, 63, 64 } : new List<int> { 1, 8, 9 };
            var planEvents = controllerEventLogRepository.GetSignalEventsByEventCode(
                approach.SignalId,
                options.Start,
                options.End,
                131).OrderBy(e => e.Timestamp).ToList();
            var detectorEvents = new List<ControllerEventLog>();
            if (options.GetVolume)
            {
                var channels = approachDetectors.Select(a => a.DetChannel).ToList();
                detectorEvents = controllerEventLogRepository.GetRecordsByParameterAndEvent(
                    approach.SignalId,
                    options.Start,
                    options.End,
                    channels,
                    new List<int> { 82 })
                    .OrderBy(e => e.Timestamp)
                    .ToList();
            }
            var cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(
                approach.SignalId,
                options.Start.AddSeconds(-900),
                options.End.AddSeconds(900),
                cycleEventCodes,
                options.GetPermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber).OrderBy(e => e.Timestamp).ToList();
            var signalPhase = signalPhaseService.GetSignalPhaseData(
                options.Start,
                options.End,
                options.GetPermissivePhase,
                false,
                null,
                options.BinSize,
                approach,
                cycleEvents,
                planEvents,
                detectorEvents); 
            ApproachDelayResult viewModel = approachDelayService.GetChartData(
                options,
                approach,
                signalPhase);
            return viewModel;
        }



    }
}
