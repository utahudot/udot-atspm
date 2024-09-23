using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Segments;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
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
            var segments = segmentRepository.AllSegmentsWithEntity();

            var features = new List<Feature>();
            foreach (var lookedUpSegment in segments)
            {
                var geometry = lookedUpSegment.Shape;
                var segment = new AttributesTable
                {
                    { "Id", lookedUpSegment.Id },
                    { "UdotRouteNumber", lookedUpSegment.UdotRouteNumber },
                    { "StartMilePoint", lookedUpSegment.StartMilePoint },
                    { "EndMilePoint", lookedUpSegment.EndMilePoint },
                    { "FunctionalType", lookedUpSegment.FunctionalType },
                    { "Name", lookedUpSegment.Name },
                    { "Direction", lookedUpSegment.Direction },
                    { "SpeedLimit", lookedUpSegment.SpeedLimit },
                    { "Region", lookedUpSegment.Region },
                    { "City", lookedUpSegment.City },
                    { "County", lookedUpSegment.County },
                    { "AlternateIdentifier", lookedUpSegment.AlternateIdentifier },
                    { "AccessCategory", lookedUpSegment.AccessCategory },
                    { "Offset", lookedUpSegment.Offset },
                    { "RouteEntities", lookedUpSegment.Entities }
                };
                var feature = new Feature(geometry, segment);
                features.Add(feature);
            }

            var featureCollection = new FeatureCollection();
            features.ForEach(f => featureCollection.Add(f));
            var featuresArray = featureCollection.ToArray();

            var geoJsonObj = new
            {
                type = "Segments",
                features = featuresArray
            };


            var writer = new GeoJsonWriter();
            var geoJson = writer.Write(geoJsonObj);
            //var test = System.IO.File.ReadAllText(geoJson);


            // Return the GeoJSON as the response
            return Content(geoJson, "application/geo+json");
            //return Ok(segments);
        }

        // POST: /Segment
        [HttpPost("")]
        public async Task<ActionResult<List<Segment>>> GetAllSegmentsInList(
            [FromBody] List<Guid> segmentIds)
        {
            //Get the segments
            List<Segment> segments = await segmentRepository.GetSegmentsDetailsWithEntity(segmentIds);
            //segments.ForEach(seg => seg.Shape = null);
            var features = new List<Feature>();
            foreach (var lookedUpSegment in segments)
            {
                var geometry = lookedUpSegment.Shape;
                var segment = new AttributesTable
                {
                    { "Id", lookedUpSegment.Id },
                    { "UdotRouteNumber", lookedUpSegment.UdotRouteNumber },
                    { "StartMilePoint", lookedUpSegment.StartMilePoint },
                    { "EndMilePoint", lookedUpSegment.EndMilePoint },
                    { "FunctionalType", lookedUpSegment.FunctionalType },
                    { "Name", lookedUpSegment.Name },
                    { "Direction", lookedUpSegment.Direction },
                    { "SpeedLimit", lookedUpSegment.SpeedLimit },
                    { "Region", lookedUpSegment.Region },
                    { "City", lookedUpSegment.City },
                    { "County", lookedUpSegment.County },
                    { "AlternateIdentifier", lookedUpSegment.AlternateIdentifier },
                    { "AccessCategory", lookedUpSegment.AccessCategory },
                    { "Offset", lookedUpSegment.Offset },
                    { "RouteEntities", lookedUpSegment.Entities }
                };
                var feature = new Feature(geometry, segment);
                features.Add(feature);
            }

            var featureCollection = new FeatureCollection();
            features.ForEach(f => featureCollection.Add(f));
            var featuresArray = featureCollection.ToArray();

            var geoJsonObj = new
            {
                type = "Segments",
                features = featuresArray
            };


            var writer = new GeoJsonWriter();
            var geoJson = writer.Write(geoJsonObj);
            //var test = System.IO.File.ReadAllText(geoJson);


            // Return the GeoJSON as the response
            return Content(geoJson, "application/geo+json");
            //return Ok(segments);
        }

        // GET: /Segment/segment/{segmentId}
        [HttpGet("{segmentId}")]
        public async Task<IActionResult> GetSegment(
            Guid segmentId)
        {
            //Get the segments
            var lookedUpSegment = await segmentRepository.LookupAsync(segmentId);
            var geometry = lookedUpSegment.Shape;
            lookedUpSegment.Shape = null;

            var segment = new AttributesTable
            {
                { "Id", lookedUpSegment.Id },
                { "UdotRouteNumber", lookedUpSegment.UdotRouteNumber },
                { "StartMilePoint", lookedUpSegment.StartMilePoint },
                { "EndMilePoint", lookedUpSegment.EndMilePoint },
                { "FunctionalType", lookedUpSegment.FunctionalType },
                { "Name", lookedUpSegment.Name },
                { "Direction", lookedUpSegment.Direction },
                { "SpeedLimit", lookedUpSegment.SpeedLimit },
                { "Region", lookedUpSegment.Region },
                { "City", lookedUpSegment.City },
                { "County", lookedUpSegment.County },
                { "AlternateIdentifier", lookedUpSegment.AlternateIdentifier },
                { "AccessCategory", lookedUpSegment.AccessCategory },
                { "Offset", lookedUpSegment.Offset },
                { "RouteEntities", lookedUpSegment.Entities }
            };

            var features = new List<Feature>();
            var feature = new Feature(geometry, segment);
            features.Add(feature);
            var featureCollection = new FeatureCollection();
            features.ForEach(f => featureCollection.Add(f));
            var featuresArray = featureCollection.ToArray();

            var geoJsonObj = new
            {
                type = "Segment",
                features = featuresArray
            };


            var writer = new GeoJsonWriter();
            var geoJson = writer.Write(geoJsonObj);

            return Content(geoJson, "application/geo+json");
        }

        // POST: /Segment/speeds
        [HttpPost("speeds")]
        public async Task<ActionResult<List<HourlySpeed>>> GetAllSegmentsSpeedsInList(
            [FromBody] SegmentRequestDto segmentRequestDto)
        {
            //Get the segments
            var segments = await segmentRepository.GetSegmentsDetailsWithEntity(segmentRequestDto.SegmentIds);
            //segments.ForEach(seg => seg.Shape = null);

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