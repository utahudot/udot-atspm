using ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using Microsoft.AspNetCore.Mvc;
using SpeedManagementApi.Processors;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // POST: /MonthlyAggregation/segments
        [HttpPost("segments")]
        public async Task<ActionResult<List<SpeedOverDistanceDto>>> GetSegmentsMonthlyAggregations([FromBody] SpeedOverDistanceRequestDto speedOverDistanceRequest)
        {
            var thresholdDate = DateTime.UtcNow.AddYears(-2).AddMonths(-1);
            if (speedOverDistanceRequest == null || speedOverDistanceRequest.StartDate < thresholdDate || speedOverDistanceRequest.StartDate > speedOverDistanceRequest.EndDate)
            {
                return BadRequest();
            }
            List<SpeedOverDistanceDto> speedOverDistances = await monthlyAggregationService.MonthlyAggregationsForSegmentInTimePeriod(speedOverDistanceRequest.SegmentIds, speedOverDistanceRequest.StartDate, speedOverDistanceRequest.EndDate);

            return Ok(speedOverDistances);
        }

    }
}