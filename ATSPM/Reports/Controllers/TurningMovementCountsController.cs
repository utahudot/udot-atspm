using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.TurningMovementCounts;
using ATSPM.Application.Repositories;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurningMovementCountsController : ControllerBase
    {
        private readonly IApproachRepository approachRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly TurningMovementCountsService turningMovementCountsService;

        public TurningMovementCountsController(
            IApproachRepository approachRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            TurningMovementCountsService turningMovementCountsService
            )
        {
            this.approachRepository = approachRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.turningMovementCountsService = turningMovementCountsService;
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
        public TurningMovementCountsResult GetChartData([FromBody] TurningMovementCountsOptions options)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            var movementTypes = options.MovementTypes.Select(m => (int)m);
            var planEvents = controllerEventLogRepository.GetPlanEvents(
                approach.SignalId,
                options.Start,
                options.End);
            var detectors = approach.GetDetectorsForMetricType(5)
                .Where(d => d.LaneType.Id == options.LaneType && movementTypes.Contains((int)d.MovementType.Id)).ToList();
            var detectorChannels = detectors.Select(d => d.DetChannel).ToList();
            var events = controllerEventLogRepository.GetRecordsByParameterAndEvent(approach.SignalId, options.Start,
                        options.End, new List<int> { 82 }, detectorChannels).ToList();
            var result = turningMovementCountsService.GetChartData(
                options,
                approach,
                detectors,
                events,
                planEvents.ToList());
            return result;
        }

    }
}
