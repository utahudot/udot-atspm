using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.ApproachVolume;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

//For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachVolumeController : ControllerBase
    {
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ApproachVolumeService approachVolumeService;

        public ApproachVolumeController(
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            ApproachVolumeService approachVolumeService)
        {
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachVolumeService = approachVolumeService;
        }

        //GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public ApproachVolumeResult Test()
        {
            Fixture fixture = new();
            ApproachVolumeResult approachVolumeViewModel = fixture.Create<ApproachVolumeResult>();
            return approachVolumeViewModel;
        }


        [HttpPost("getChartData")]
        public ApproachVolumeResult GetChartData([FromBody] ApproachVolumeOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId);
            var primaryApproaches = signal.Approaches.Where(a => a.DirectionTypeId == options.Direction).ToList();
            int primaryDistanceFromStopBar = 0;
            int opposingDistanceFromStopBar = 0;
            List<ControllerEventLog> primaryDetectorEvents = GetDetectorEvents(options, signal, true, out primaryDistanceFromStopBar);
            List<ControllerEventLog> opposingDetectorEvents = GetDetectorEvents(options, signal, false, out opposingDistanceFromStopBar);
            if (primaryDetectorEvents.Count == 0 && opposingDetectorEvents.Count == 0)
            {
                return new ApproachVolumeResult(primaryApproaches.FirstOrDefault().Id, signal.SignalIdentifier, options.Start, options.End);
            }
            ApproachVolumeResult viewModel = approachVolumeService.GetChartData(
                options,
                signal,
                primaryDetectorEvents,
                opposingDetectorEvents,
                primaryDistanceFromStopBar);
            return viewModel;
        }

        private List<ControllerEventLog> GetDetectorEvents(ApproachVolumeOptions options, Signal signal, bool usePrimaryDirection, out int distanceFromStopBar)
        {
            var approaches = signal.Approaches
                .Where(a => a.DirectionTypeId == (usePrimaryDirection ? options.Direction : ApproachVolumeService.GetOpposingDirection(options)))
                .ToList();
            var detectors = approaches
                .SelectMany(a => a.Detectors)
                .Where(d => d.DetectionTypes.Any(dt => dt.Id == options.DetectionType) && (d.LaneTypeId == LaneTypes.V || d.LaneTypeId == LaneTypes.NA))
                .ToList();
            distanceFromStopBar = 0;
            if (detectors.Any())
            {
                distanceFromStopBar = detectors.First().DistanceFromStopBar ?? 0;
            }

            var detectorEvents = detectors.SelectMany(d => controllerEventLogRepository.GetEventsByEventCodesParam(
                d.Approach.Signal.SignalIdentifier,
                options.Start,
                options.End,
                new List<int> { 82 },
                d.DetChannel,
                d.GetOffset(),
                d.LatencyCorrection)).ToList();
            return detectorEvents;
        }
    }
}
