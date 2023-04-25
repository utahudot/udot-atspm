using ATSPM.Application.Reports.Business.SplitMonitor;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SplitMonitorController : ControllerBase
    {
        private readonly SplitMonitorService splitMonitorService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public SplitMonitorController(
            SplitMonitorService splitMonitorService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.splitMonitorService = splitMonitorService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public SplitMonitorResult Test()
        {
            Fixture fixture = new();
            SplitMonitorResult viewModel = fixture.Create<SplitMonitorResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public SplitMonitorResult GetChartData([FromBody] SplitMonitorOptions options)
        {
            var signal = signalRepository.GetVersionOfSignalByDate(options.SignalId, options.Start);
            var phaseEvents = controllerEventLogRepository.GetSignalEventsByEventCodes(options.SignalId, options.Start, options.End,
                new List<int> { 1, 11, 4, 5, 6, 7, 21, 23 });
            var planEvents = controllerEventLogRepository.GetPlanEvents(
                options.SignalId,
                options.Start,
                options.End);
            SplitMonitorResult viewModel = splitMonitorService.GetChartData(
                options,
                planEvents,
                phaseEvents,
                signal);
            return viewModel;
        }

    }
}
