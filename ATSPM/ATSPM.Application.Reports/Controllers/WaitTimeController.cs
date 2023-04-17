using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.WaitTime;
using ATSPM.Application.Repositories;
using AutoFixture;
using Legacy.Common.Business;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaitTimeController : ControllerBase
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly IApproachRepository approachRepository;
        private readonly WaitTimeService waitTimeService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public WaitTimeController(
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            IApproachRepository approachRepository,
            WaitTimeService waitTimeService,
            IControllerEventLogRepository controllerEventLogRepository
            )
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.approachRepository = approachRepository;
            this.waitTimeService = waitTimeService;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public WaitTimeResult Test()
        {
            Fixture fixture = new();
            WaitTimeResult viewModel = fixture.Create<WaitTimeResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public WaitTimeResult GetChartData([FromBody] WaitTimeOptions options)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            var detectorChannels = approach.GetDetectorsForMetricType(32).Select(d => d.DetChannel).ToList();
            var planEvents = controllerEventLogRepository.GetPlanEvents(
                approach.SignalId,
                options.Start,
                options.End);
            var events = controllerEventLogRepository.GetSignalEventsByEventCodes(
                approach.SignalId,
                options.Start,
                options.End,
                new List<int>() { 
                    82,
                    WaitTimeOptions.PHASE_BEGIN_GREEN,
                    WaitTimeOptions.PHASE_CALL_DROPPED,
                    WaitTimeOptions.PHASE_END_RED_CLEARANCE,
                    WaitTimeOptions.PHASE_CALL_REGISTERED}
                ).ToList();
            var volume = new VolumeCollection(
                options.Start,
                options.End,
                events.Where(e => e.EventCode == 82).ToList(),
                options.BinSize);
            var analysisPhaseDataCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                approach.SignalId,
                options.Start,
                options.End,
                planEvents.ToList());
            var analysisPhaseData = analysisPhaseDataCollection.AnalysisPhases.Where(a => a.PhaseNumber == approach.ProtectedPhaseNumber).FirstOrDefault();
            return waitTimeService.GetChartData(
                options,
                approach,
                events,
                analysisPhaseDataCollection.AnalysisPhases.Where(a => a.PhaseNumber == approach.ProtectedPhaseNumber).First(),
                analysisPhaseDataCollection.Plans,
                volume
                );

        }
    }
}
