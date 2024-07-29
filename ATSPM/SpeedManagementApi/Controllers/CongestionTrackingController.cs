using Asp.Versioning;
using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
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

            try
            {
                var result = await _congestionTrackingService.GetReportData(options);
                return Ok(result);
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
            }
            return Unauthorized("I am here");
        }
    }
}
