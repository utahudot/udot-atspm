using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedFromImpact;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpeedFromImpactController : ControllerBase
    {
        private ImpactService impactService;
        private readonly ISegmentRepository segmentRepository;
        private readonly HourlySpeedService hourlySpeedService;

        public SpeedFromImpactController(ImpactService impactService, ISegmentRepository segmentRepository, HourlySpeedService hourlySpeedService)
        {
            this.impactService = impactService;
            this.segmentRepository = segmentRepository;
            this.hourlySpeedService = hourlySpeedService;
        }

        //With the impact get all the segments that are affected
        // POST: /SpeedFromImpact/impact/{impactId}
        [HttpPost("impact/{impactId}")]
        public async Task<ActionResult<List<SpeedFromImpactDto>>> GetImpactsOnSpeed(
            Guid impactId,
            [FromBody] SpeedFromImpactDto speedFromImpact)
        {
            //Get the impacts
            Impact impact = await impactService.GetImpactById(impactId);
            speedFromImpact.Impacts = new List<Impact> { impact };

            //Get the segments
            List<Segment> segments = await segmentRepository.GetSegmentsDetails(impact.SegmentIds);
            segments.ForEach(seg => seg.Shape = null);
            speedFromImpact.Segments = segments;

            //get the hourly speed for those segments based on the date times.
            List<HourlySpeed> hourlySpeeds = new List<HourlySpeed>();
            DateTime startTime = new DateTime(1, 1, 1, 0, 0, 0); // 12:00 AM
            DateTime endTime = new DateTime(1, 1, 1, 23, 59, 59); // 11:59 PM
            foreach (var segment in segments)
            {
                var currentSegments = await hourlySpeedService.GetHourlySpeedsForTimePeriod(segment.Id, speedFromImpact.StartDate, speedFromImpact.EndDate, startTime, endTime);
                hourlySpeeds.AddRange(currentSegments);
            }
            speedFromImpact.HourlySpeeds = hourlySpeeds;
            return Ok(speedFromImpact);
        }

        //With a segement get all the impacts
        // POST: /SpeedFromImpact/segment/{segmentId}
        [HttpPost("segment/{segmentId}")]
        public async Task<ActionResult<List<SpeedFromImpactDto>>> GetAllImpactsOnSpeedFromASegment(
            Guid segmentId,
            [FromBody] SpeedFromImpactDto speedFromImpact)
        {
            //Get the segments
            Segment segment = await segmentRepository.LookupAsync(segmentId);
            segment.Shape = null;
            speedFromImpact.Segments = new List<Segment> { segment };

            //Get the impacts
            List<Impact> impacts = await impactService.GetImpactsOnSegment(segmentId);
            speedFromImpact.Impacts = impacts;

            //get the hourly speed for those segments based on the date times.
            DateTime startTime = new DateTime(1, 1, 1, 0, 0, 0); // 12:00 AM
            DateTime endTime = new DateTime(1, 1, 1, 23, 59, 59); // 11:59 PM

            speedFromImpact.HourlySpeeds = await hourlySpeedService.GetHourlySpeedsForTimePeriod(segmentId, speedFromImpact.StartDate, speedFromImpact.EndDate, startTime, endTime);

            return Ok(speedFromImpact);
        }

        ////given a list of segments and a list of impacts get the data
        //// POST: /SpeedFromImpact
        //[HttpPost("")]
        //public async Task<ActionResult<List<SpeedFromImpactDto>>> GetSpeedsFromImpact(
        //    [FromBody] SpeedFromImpactDto speedFromImpact)
        //{
        //    if (speedFromImpact.ImpactIds == null)
        //    {
        //        return BadRequest();
        //    }
        //    //Get the impacts
        //    List<Impact> impacts = await impactService.GetListOfImpactsFromIds(speedFromImpact.ImpactIds);
        //    speedFromImpact.Impacts = impacts;

        //    //Get the segments
        //    List<Segment> segments = await segmentRepository.GetSegmentsDetails(speedFromImpact.SegmentIds);
        //    speedFromImpact.Segments = segments;

        //    //get the hourly speed for those segments based on the date times.
        //    List<HourlySpeed> hourlySpeeds = new List<HourlySpeed>();
        //    DateTime startTime = new DateTime(1, 1, 1, 0, 0, 0); // 12:00 AM
        //    DateTime endTime = new DateTime(1, 1, 1, 23, 59, 59); // 11:59 PM
        //    foreach (var segment in segments)
        //    {
        //        var currentSegments = await hourlySpeedService.GetHourlySpeedsForTimePeriod(segment.Id, speedFromImpact.StartDate, speedFromImpact.EndDate, startTime, endTime);
        //        hourlySpeeds.AddRange(currentSegments);
        //    }
        //    speedFromImpact.HourlySpeeds = hourlySpeeds;
        //    return Ok(speedFromImpact);
        //}

    }
}