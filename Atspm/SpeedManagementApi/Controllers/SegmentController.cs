using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Segments;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SegmentController : ControllerBase
    {
        private readonly ISegmentRepository segmentRepository;
        private readonly HourlySpeedService hourlySpeedService;

        public SegmentController(ISegmentRepository segmentRepository, HourlySpeedService hourlySpeedService)
        {
            this.segmentRepository = segmentRepository;
            this.hourlySpeedService = hourlySpeedService;
        }

        // GET: /Segment
        [HttpGet("")]
        public async Task<ActionResult<List<Segment>>> GetAllSegments()
        {
            //Get the segments
            var segments = segmentRepository.GetList().ToList();
            segments.ForEach(seg => seg.Shape = null);

            return Ok(segments);
        }

        // POST: /Segment
        [HttpPost("")]
        public async Task<ActionResult<List<Segment>>> GetAllSegmentsInList(
            [FromBody] List<Guid> segmentIds)
        {
            //Get the segments
            List<Segment> segments = await segmentRepository.GetSegmentsDetails(segmentIds);
            segments.ForEach(seg => seg.Shape = null);

            return Ok(segments);
        }

        // GET: /Segment/segment/{segmentId}
        [HttpGet("{segmentId}")]
        public async Task<ActionResult<Segment>> GetSegment(
            Guid segmentId)
        {
            //Get the segments
            var segment = await segmentRepository.LookupAsync(segmentId);
            segment.Shape = null;

            return Ok(segment);
        }

        // POST: /Segment/speeds
        [HttpPost("speeds")]
        public async Task<ActionResult<List<HourlySpeed>>> GetAllSegmentsSpeedsInList(
            [FromBody] SegmentRequestDto segmentRequestDto)
        {
            //Get the segments
            var segments = await segmentRepository.GetSegmentsDetails(segmentRequestDto.SegmentIds);
            segments.ForEach(seg => seg.Shape = null);

            //get the hourly speed for those segments based on the date times.
            List<HourlySpeed> hourlySpeeds = new List<HourlySpeed>();
            DateTime startTime = new DateTime(1, 1, 1, 0, 0, 0); // 12:00 AM
            DateTime endTime = new DateTime(1, 1, 1, 23, 59, 59); // 11:59 PM
            foreach (var segment in segments)
            {
                var currentSegments = await hourlySpeedService.GetHourlySpeedsForTimePeriod(segment.Id, segmentRequestDto.StartDate, segmentRequestDto.EndDate, startTime, endTime);
                hourlySpeeds.AddRange(currentSegments);
            }

            return Ok(hourlySpeeds);
        }

        // POST: /Segment/{segmentId}/speeds
        [HttpPost("{segmentId}/speeds")]
        public async Task<ActionResult<List<HourlySpeed>>> GetSegmentSpeeds(
            Guid segmentId,
            [FromBody] SegmentRequestDto segmentRequestDto)
        {
            //Get the segments
            var segment = await segmentRepository.LookupAsync(segmentId);
            segment.Shape = null;

            //get the hourly speed for those segments based on the date times.
            DateTime startTime = new DateTime(1, 1, 1, 0, 0, 0); // 12:00 AM
            DateTime endTime = new DateTime(1, 1, 1, 23, 59, 59); // 11:59 PM
            var currentSegments = await hourlySpeedService.GetHourlySpeedsForTimePeriod(segment.Id, segmentRequestDto.StartDate, segmentRequestDto.EndDate, startTime, endTime);

            return Ok(currentSegments);
        }
    }
}