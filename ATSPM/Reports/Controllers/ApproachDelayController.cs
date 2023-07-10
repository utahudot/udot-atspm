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
            var approach = approachRepository.GetList().Where(a => a.Id == options.ApproachId).FirstOrDefault();
            var planEvents = controllerEventLogRepository.GetPlanEvents(
                approach.Signal.SignalId,
                options.Start,
                options.End);
            int distanceFromStopBar = 0;
            var detectorEvents = controllerEventLogRepository.GetDetectorEvents(8, approach, options.Start, options.End, true, false).ToList(); 
            var cycleEvents = controllerEventLogRepository.GetCycleEventsWithTimeExtension(
                approach,
                options.GetPermissivePhase,
                options.Start,
                options.End);
                
            var signalPhase = signalPhaseService.GetSignalPhaseData(
                options.Start,
                options.End,
                false,
                null,
                options.BinSize,
                approach,
                cycleEvents.ToList(),
                planEvents.ToList(),
                detectorEvents); 
            ApproachDelayResult viewModel = approachDelayService.GetChartData(
                options,
                approach,
                signalPhase);
            return viewModel;
        }



    }
}
