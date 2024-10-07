using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SpeedManagementApi.Processors;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MonthlyAggregationController : ControllerBase
    {
        private MonthlyAggregationService monthlyAggregationService;
        private DeleteOldEventsProcessor deleteOldEventsProcessor;
        private AggregateMonthlyEventsProcessor aggregateMonthlyEventsProcessor;

        public MonthlyAggregationController(MonthlyAggregationService monthlyAggrectionService, DeleteOldEventsProcessor deleteOldEventsProcessor, AggregateMonthlyEventsProcessor aggregateMonthlyEventsProcessor)
        {
            this.monthlyAggregationService = monthlyAggrectionService;
            this.deleteOldEventsProcessor = deleteOldEventsProcessor;
            this.aggregateMonthlyEventsProcessor = aggregateMonthlyEventsProcessor;
        }

        // POST: /MonthlyAggregation
        [HttpPost("")]
        public async Task AggregateMonthlyEventsAsync()
        {
            await aggregateMonthlyEventsProcessor.AggregateMonthlyEvents();
            return;
        }

        // GET: /MonthlyAggregation
        [HttpGet("latest/{monthAggClassification}/{timePeriod}")]
        public async Task<ActionResult<IReadOnlyList<MonthlyAggregationSimplified>>> LatestOfEachSegmentId(TimePeriodFilter timePeriod, MonthAggClassification monthAggClassification)
        {
            IReadOnlyList<MonthlyAggregationSimplified> monthlyAggregationsForSegment = await monthlyAggregationService.LatestOfEachSegmentId(timePeriod, monthAggClassification);
            return Ok(monthlyAggregationsForSegment);
        }

        // POST: /MonthlyAggregation
        [HttpPost("hotspots")]
        public async Task<ActionResult<IReadOnlyList<RouteSpeed>>> GetTopMonthlyAggregationsInCategory(MonthlyAggregationOptions monthlyAggregationOptions)
        {
            IReadOnlyList<RouteSpeed> monthlyAggregationsForSegment = await monthlyAggregationService.GetTopMonthlyAggregationsInCategory(monthlyAggregationOptions);
            var features = new List<Feature>();
            foreach (var routeSpeed in monthlyAggregationsForSegment)
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

        // POST: /MonthlyAggregation
        [HttpPost("source/{sourceId}")]
        public async Task AggregateCertainMonthforSource(int sourceId, [FromBody] DateTime date)
        {
            await aggregateMonthlyEventsProcessor.AggregateCertainMonthforSource(date, sourceId);
            return;
        }

        // POST: /MonthlyAggregation/segment
        [HttpPost("segment")]
        public async Task AggregateMonthlyEventsForSingleSegmentAsync([FromBody] MonthlyAggregation monthlyAggregation)
        {
            await aggregateMonthlyEventsProcessor.AggregateMonthlyEventsForSingleSegment(monthlyAggregation);
            return;
        }

        // Delete: /MonthlyAggregation
        [HttpDelete("")]
        public async Task DeleteOldEventsAsync()
        {
            await deleteOldEventsProcessor.DeleteOldEvents();
            return;
        }

        // GET: /MonthlyAggregation/segments/{id}
        [HttpGet("segments/{segmentId}/{monthAggClassification}/{timePeriod}")]
        public async Task<ActionResult<IReadOnlyList<MonthlyAggregationSimplified>>> GetMonthlyAggregationForSegment(Guid segmentId, TimePeriodFilter timePeriod, MonthAggClassification monthAggClassification)
        {
            IReadOnlyList<MonthlyAggregationSimplified> monthlyAggregationsForSegment = await monthlyAggregationService.ListMonthlyAggregationsForSegment(segmentId, timePeriod, monthAggClassification);
            return Ok(monthlyAggregationsForSegment);
        }

        [HttpGet("filtering-time-periods")]
        public IActionResult GetFilteringTimePeriodMapping()
        {
            var mappings = Enum.GetValues(typeof(TimePeriodFilter))
                .Cast<TimePeriodFilter>()
                .Select(e => new EnumMapping
                {
                    Number = (int)e,
                    DisplayName = e.GetDisplayName()
                })
                .ToList();

            return Ok(mappings);
        }

        [HttpGet("month-agg-classifications")]
        public IActionResult GetMonthAggClassificationMapping()
        {
            var mappings = Enum.GetValues(typeof(MonthAggClassification))
                .Cast<MonthAggClassification>()
                .Select(e => new EnumMapping
                {
                    Number = (int)e,
                    DisplayName = e.GetDisplayName()
                })
                .ToList();

            return Ok(mappings);
        }

        [HttpGet("speed-category-filters")]
        public IActionResult GetSpeedCategoryFilterMapping()
        {
            var mappings = Enum.GetValues(typeof(SpeedCategoryFilter))
                .Cast<SpeedCategoryFilter>()
                .Select(e => new EnumMapping
                {
                    Number = (int)e,
                    DisplayName = e.GetDisplayName()
                })
                .ToList();

            return Ok(mappings);
        }
    }
}