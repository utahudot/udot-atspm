using ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpeedOverDistanceController : ControllerBase
    {
        private MonthlyAggregationService monthlyAggregationService;

        public SpeedOverDistanceController(MonthlyAggregationService monthlyAggregationService)
        {
            this.monthlyAggregationService = monthlyAggregationService;
        }

        // POST: /SpeedOverDistance
        [HttpPost("")]
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