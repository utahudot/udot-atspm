using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CongestionTrackingController : ControllerBase
    {
        private ICongestionTrackingService _congestionTrackingService;
        public CongestionTrackingController(ICongestionTrackingService congestionTrackingService)
        {
            _congestionTrackingService = congestionTrackingService;
        }

        [HttpPost("GetReportData")]
        public async Task<IActionResult> GetReportData([FromBody] CongestionTrackingOptions options)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _congestionTrackingService.GetReportData(options);
            return Ok(result);
        }
    }
}
