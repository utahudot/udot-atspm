using ATSPM.Application.Reports.Business.SplitMonitor;
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
    public class SplitMonitorController : ControllerBase
    {
        private readonly SplitMonitorService splitMonitorService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public SplitMonitorController(
            SplitMonitorService splitMonitorService,
            IControllerEventLogRepository controllerEventLogRepository)
        {
            this.splitMonitorService = splitMonitorService;
            this.controllerEventLogRepository = controllerEventLogRepository;
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
            var planEvents = controllerEventLogRepository.GetPlanEvents(
                options.SignalId,
                options.Start,
                options.End);
            SplitMonitorResult viewModel = splitMonitorService.GetChartData(
                options,
                planEvents));
            return viewModel;
        }

    }
}
