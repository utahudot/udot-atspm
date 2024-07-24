using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    public class CongestionTrackingController : ControllerBase
    {
        private ICongestionTrackingService _congestionTrackingService;
        public CongestionTrackingController(ICongestionTrackingService congestionTrackingService)
        {
            _congestionTrackingService = congestionTrackingService;
        }

        [HttpPost("GetReportData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReportData([FromBody] CongestionTrackingOptions options)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _congestionTrackingService.GetReportData(options);
                return Ok(result);
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
            }
            return BadRequest(ModelState);
        }
    }
}
