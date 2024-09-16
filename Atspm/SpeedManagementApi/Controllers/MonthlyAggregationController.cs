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
        [HttpGet("segments/{segmentId}")]
        public async Task<ActionResult<IReadOnlyList<MonthlyAggregation>>> GetMonthlyAggregationForSegment(Guid segmentId)
        {
            IReadOnlyList<MonthlyAggregation> monthlyAggregationsForSegment = await monthlyAggregationService.ListMonthlyAggregationsForSegment(segmentId);
            return Ok(monthlyAggregationsForSegment);
        }

    }
}