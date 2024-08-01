using ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MonthlyAggregationController : ControllerBase
    {
        private MonthlyAggregationService monthlyAggregationService;

        public MonthlyAggregationController(MonthlyAggregationService monthlyAggrectionService)
        {
            this.monthlyAggregationService = monthlyAggrectionService;
        }

        // GET: /ImpactType/{segmentId}
        [HttpGet("{segmentId}")]
        public async Task<ActionResult<List<MonthlyAggregation>>> GetImpactTypeById(Guid segmentId)
        {
            var impactType = await monthlyAggregationService.ListMonthlyAggregationsForSegment(segmentId);
            if (impactType == null)
            {
                return NotFound();
            }
            return Ok(impactType);
        }

    }
}