using Microsoft.AspNetCore.Mvc;
using SpeedManagementApi.Processors;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
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
        public async Task<ActionResult<IReadOnlyList<MonthlyAggregationSimplified>>> GetTopMonthlyAggregationsInCategory(MonthlyAggregationOptions monthlyAggregationOptions)
        {
            IReadOnlyList<MonthlyAggregationSimplified> monthlyAggregationsForSegment = await monthlyAggregationService.GetTopMonthlyAggregationsInCategory(monthlyAggregationOptions);
            return Ok(monthlyAggregationsForSegment);
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