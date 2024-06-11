using ATSPM.Application.Business.RouteSpeed;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpeedManagementController: ControllerBase
    {
        private RouteSpeedService routeSpeedService;
        private RouteService routeService;

        public SpeedManagementController(RouteSpeedService routeSpeedService, RouteService routeService)
        {
            this.routeSpeedService = routeSpeedService;
            this.routeService = routeService;
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] RouteSpeedOptions item)
        {
            await this.routeSpeedService.AddPemsSpeed(item);

            return Ok();
        }

        [HttpPut("PutSpeedRoutes")]
        public async Task<IActionResult> PutSpeedAsync([FromBody] RouteSpeedOptions item)
        {
            await this.routeSpeedService.AddTestSpeedsPerRoute();

            return Ok();
        }

        [HttpPut("PutTestRoutes")]
        public async Task<IActionResult> Put([FromBody] RouteSpeedOptions item)
        {
            await this.routeService.AddRandomRoutes();

            return Ok();
        }

        [HttpPost("GetRouteSpeeds", Name = "GetRouteSpeeds")]
        public async Task<IActionResult> GetRoutesSpeeds([FromBody] RouteSpeedOptions options)
        {

            IEnumerable<RouteSpeed> routeSpeeds = await routeSpeedService.GetRouteSpeedsAsync(options);

            var features = new List<Feature>();
            foreach (var routeSpeed in routeSpeeds)
            {
                var geometry = routeSpeed.Shape;
                var properties = new AttributesTable
                {
                    { "route_id", routeSpeed.RouteId },
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

        [HttpPut("GetHistoricalSpeeds")]
        public async Task<IActionResult> GetHistoricalData()
        {
            var task = this.routeSpeedService.GetHistoricalSpeeds(1, new DateOnly(2024, 5, 1), new DateOnly(2024, 5, 31),  "0,1,2,3,4,5,6,7", 10, AnalysisPeriod.AllDay);

            var result = await Task.Run(() => task);

            if(result != null)
            {
                return Ok(result);
            } else { return BadRequest(); }
        }

    }
}
