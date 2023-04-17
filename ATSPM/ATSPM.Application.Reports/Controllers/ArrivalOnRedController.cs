using ATSPM.Application.Reports.Business.ArrivalOnRed;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArrivalOnRedController : ControllerBase
    {
        private readonly ArrivalOnRedService arrivalOnRedService;
        private readonly SignalPhaseService signalPhaseService;
        private readonly IApproachRepository approachRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public ArrivalOnRedController(
            ArrivalOnRedService arrivalOnRedService,
            SignalPhaseService signalPhaseService,
            IApproachRepository approachRepository,
            IControllerEventLogRepository controllerEventLogRepository
            )
        {
            this.arrivalOnRedService = arrivalOnRedService;
            this.signalPhaseService = signalPhaseService;
            this.approachRepository = approachRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public ArrivalOnRedResult Test()
        {
            Fixture fixture = new();
            ArrivalOnRedResult viewModel = fixture.Create<ArrivalOnRedResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public ArrivalOnRedResult GetChartData([FromBody] ArrivalOnRedOptions options)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            var planEvents = controllerEventLogRepository.GetPlanEvents(
                approach.SignalId,
                options.Start,
                options.End);
            var detectorEvents = controllerEventLogRepository.GetDetectorEvents(8, approach, options.Start, options.End, true, false);
            var cycleEvents = controllerEventLogRepository.GetCycleEventsWithTimeExtension(
                approach,
                options.UsePermissivePhase,
                options.Start,
                options.End);
            var signalPhase = signalPhaseService.GetSignalPhaseData(
                options.Start,
                options.End,
                false,
                false,
                null,
                options.SelectedBinSize,
                //9,
                approach,
                cycleEvents.ToList(),
                planEvents.ToList(),
                detectorEvents.ToList()
                );
            ArrivalOnRedResult viewModel = arrivalOnRedService.GetChartData(options, signalPhase, approach);
            return viewModel;
        }

    }
}
