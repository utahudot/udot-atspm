using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.LeftTurnGapAnalysis;
using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerdueCoordinationDiagramController : ControllerBase
    {
        private readonly PerdueCoordinationDiagramService perdueCoordinationDiagramService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly IApproachRepository approachRepository;
        private readonly SignalPhaseService signalPhaseService;

        public PerdueCoordinationDiagramController(
            PerdueCoordinationDiagramService perdueCoordinationDiagramService,
            IControllerEventLogRepository controllerEventLogRepository,
            IApproachRepository approachRepository,
            SignalPhaseService signalPhaseService)
        {
            this.perdueCoordinationDiagramService = perdueCoordinationDiagramService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachRepository = approachRepository;
            this.signalPhaseService = signalPhaseService;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PerdueCoordinationDiagramResult Test()
        {
            Fixture fixture = new();
            PerdueCoordinationDiagramResult viewModel = fixture.Create<PerdueCoordinationDiagramResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public PerdueCoordinationDiagramResult GetChartData([FromBody] PerdueCoordinationDiagramOptions options)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            var events = controllerEventLogRepository.GetDetectorEvents(6, approach, options.StartDate, options.EndDate);
            var signalPhase = signalPhaseService.GetSignalPhaseData(
                options.StartDate,
                options.EndDate,
                false,
                options.ShowVolumes,
                0,
                options.SelectedBinSize,
                approach,
                events.ToList());
            PerdueCoordinationDiagramResult viewModel = perdueCoordinationDiagramService.GetChartData(options, approach, signalPhase);
            return viewModel;
        }

        

    }
}
