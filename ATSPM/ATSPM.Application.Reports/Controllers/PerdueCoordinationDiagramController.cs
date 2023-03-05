using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.LeftTurnGapAnalysis;
using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

        public PerdueCoordinationDiagramController(
            PerdueCoordinationDiagramService perdueCoordinationDiagramService,
            IControllerEventLogRepository controllerEventLogRepository,
            IApproachRepository approachRepository)
        {
            this.perdueCoordinationDiagramService = perdueCoordinationDiagramService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachRepository = approachRepository;
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
            //var events = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalId, options.StartDate, options.EndDate);
            var approach = approachRepository.Lookup(options.ApproachId);
            PerdueCoordinationDiagramResult viewModel = perdueCoordinationDiagramService.GetChartData(options);//, approach, events);
            return viewModel;
        }

    }
}
