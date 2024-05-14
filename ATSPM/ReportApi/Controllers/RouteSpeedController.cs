using ATSPM.Application.Business.RouteSpeed;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace ATSPM.ReportApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouteSpeedController : Controller
    {
        private readonly RouteSpeedService routeSpeedService;

        public RouteSpeedController(RouteSpeedService routeSpeedService)
        {
            this.routeSpeedService = routeSpeedService;
        }

        [HttpGet("Test", Name = "Test")]
        public IActionResult Test()
        {
            return Ok("test");
        }

        //[EnableCors("Policy1")]
        [HttpGet("GetRouteSpeeds", Name = "GetRouteSpeeds")]
        public IActionResult GetRoutesSpeeds(
        [FromQuery] string startDate,
        [FromQuery] string endDate,
        [FromQuery] string daysOfWeek,
        [FromQuery] int sourceId,
        [FromQuery] int violationThreshold,
        [FromQuery] int analysisPeriod,
        [FromQuery] string? startTime,
        [FromQuery] string? endTime)
        {
            var convertedStartDate = DateOnly.Parse(startDate);
            var convertedEndDate = DateOnly.Parse(endDate);
            TimeOnly.TryParse(startTime, out TimeOnly convertedStartTime);
            TimeOnly.TryParse(endTime, out TimeOnly convertedEndTime);
            IEnumerable<RouteSpeed> routeSpeeds = routeSpeedService.GetRouteSpeeds(
                convertedStartDate,
                convertedEndDate,
                convertedStartTime,
                convertedEndTime,
                daysOfWeek,
                violationThreshold,
                sourceId,
                (AnalysisPeriod)Enum.ToObject(typeof(AnalysisPeriod), analysisPeriod));
            var features = new List<Feature>();
            foreach (var routeSpeed in routeSpeeds)
            {
                var geometry = routeSpeed.Shape;
                var properties = new AttributesTable
                {
                    { "route_id", routeSpeed.RouteId },
                    { "route_name", routeSpeed.Name },
                    { "startdate", routeSpeed.Startdate?.ToString("M/d/yyyy, h:mm tt") },
                    { "enddate", routeSpeed.Enddate?.ToString("M/d/yyyy, h:mm tt") },
                    { "percentilespd_15", routeSpeed.Percentilespd_15 },
                    { "avg", routeSpeed.Avg },
                    { "percentilespd_85", routeSpeed.Percentilespd_85 },
                    { "percentilespd_95", routeSpeed.Percentilespd_95 },
                    //{ "percentilespd_99", routeSpeed.Percentilespd_99 },
                    { "averageSpeedAboveSpeedLimit", routeSpeed.AverageSpeedAboveSpeedLimit },
                    { "estimatedViolations", routeSpeed.EstimatedViolations },
                    { "flow", routeSpeed.Flow },
                    { "speedLimit", routeSpeed.SpeedLimit },
                    //{ "esri_oid", 2 },
                    //{ "SHAPE__Length", 2 }
                };
                var feature = new Feature(geometry, properties);
                features.Add(feature);
            }

            var featureCollection = new FeatureCollection();
            features.ForEach(f => featureCollection.Add(f));
            var featuresArray = featureCollection.ToArray();
            var crs = new
            {
                type = "name",
                properties = new
                {
                    name = "EPSG:4326"
                }
            };

            var geoJsonObj = new
            {
                type = "FeatureCollection",
                //crs = crs,
                features = featuresArray
            };


            var writer = new GeoJsonWriter();
            var geoJson = writer.Write(geoJsonObj);
            //var test = System.IO.File.ReadAllText(geoJson);


            // Return the GeoJSON as the response
            return Content(geoJson, "application/geo+json");
        }

        //[HttpGet("Historical", Name = "Historical")]
        //public async Task<HistoricalDTO> HistoricalMonthly(
        //[FromQuery] int routeId,
        //[FromQuery] string startDate,
        //[FromQuery] string endDate,
        //[FromQuery] string daysOfWeek,
        //[FromQuery] int analysisPeriod,
        //[FromQuery] string? startTime,
        //[FromQuery] string? endTime,
        //[FromQuery] int? percentile
        //    )
        //{
        //    var convertedStartDate = DateOnly.Parse(startDate);
        //    var convertedEndDate = DateOnly.Parse(endDate);
        //    TimeOnly.TryParse(startTime, out TimeOnly convertedStartTime);
        //    TimeOnly.TryParse(endTime, out TimeOnly convertedEndTime);

        //    var result = await routeSpeedService.GetHistoricalSpeeds(
        //        routeId,
        //        convertedStartDate,
        //        convertedEndDate,
        //        convertedStartTime,
        //        convertedEndTime,
        //        daysOfWeek,
        //        percentile ?? 1,
        //        (AnalysisPeriod)analysisPeriod);
        //    return result;
        //}
    }
}
