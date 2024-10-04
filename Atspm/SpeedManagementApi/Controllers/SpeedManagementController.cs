using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SpeedManagementController : ControllerBase
    {
        private readonly MonthlyAggregationService monthlyAggregationService;

        public SpeedManagementController(MonthlyAggregationService monthlyAggregationService)
        {
            this.monthlyAggregationService = monthlyAggregationService;
        }

        [HttpPost("GetRouteSpeeds", Name = "GetRouteSpeeds")]
        public async Task<IActionResult> GetRoutesSpeeds([FromBody] MonthlyAggregationOptions options)
        {

            IEnumerable<RouteSpeed> routeSpeeds = await monthlyAggregationService.GetRouteSpeedsAsync(options);

            var features = new List<Feature>();
            foreach (var routeSpeed in routeSpeeds)
            {
                var geometry = routeSpeed.Shape;
                var properties = new AttributesTable
                {
                    { "createdDate", routeSpeed.CreatedDate.ToString("M/d/yyyy, h:mm tt") },
                    { "binStartTime", routeSpeed.BinStartTime.ToString("M/d/yyyy, h:mm tt") },
                    { "route_id", routeSpeed.SegmentId },
                    { "sourceId", routeSpeed.SourceId },
                    { "name", routeSpeed.Name },
                    { "speedLimit", routeSpeed.SpeedLimit },
                    { "region", routeSpeed.Region },
                    { "city", routeSpeed.City },
                    { "county", routeSpeed.County },
                    { "averageSpeed", routeSpeed.AverageSpeed },
                    { "averageEightyFifthSpeed", routeSpeed.AverageEightyFifthSpeed },
                    { "violations", routeSpeed.Violations },
                    { "extremeViolations", routeSpeed.ExtremeViolations },
                    { "flow", routeSpeed.Flow },
                    { "minSpeed", routeSpeed.MinSpeed },
                    { "maxSpeed", routeSpeed.MaxSpeed },
                    { "variability", routeSpeed.Variability },
                    { "percentViolations", routeSpeed.PercentViolations },
                    { "percentExtremeViolations", routeSpeed.PercentExtremeViolations },
                    { "avgSpeedVsSpeedLimit", routeSpeed.AvgSpeedVsSpeedLimit },
                    { "eightyFifthSpeedVsSpeedLimit", routeSpeed.EightyFifthSpeedVsSpeedLimit },
                    { "percentObserved", routeSpeed.PercentObserved },
                    //{ "geometry", routeSpeed.Shape }
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

        //[HttpPost("GetHistoricalSpeeds")]
        //public async Task<IActionResult> GetHistoricalData([FromBody] HistoricalSpeedOptions options)
        //{
        //    var task = monthlyAggregationService.(options);
        //    var result = await Task.Run(() => task);GetHistoricalSpeeds


        //    if (result != null)
        //    {
        //        return Ok(result);
        //    }
        //    else { return BadRequest(); }
        //}

    }
}
