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

        [HttpPost("")]
        public async Task AggregateMonthlyEventsAsync()
        {
            await aggregateMonthlyEventsProcessor.AggregateMonthlyEvents();
            return;
        }

        [HttpPost("segment")]
        public async Task AggregateMonthlyEventsForSingleSegmentAsync([FromBody] MonthlyAggregation monthlyAggregation)
        {
            await aggregateMonthlyEventsProcessor.AggregateMonthlyEventsForSingleSegment(monthlyAggregation);
            return;
        }

        [HttpDelete("")]
        public async Task DeleteOldEventsAsync()
        {
            await deleteOldEventsProcessor.DeleteOldEvents();
            return;
        }

    }
}