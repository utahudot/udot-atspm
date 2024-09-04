using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Models.WatchDogModels;
using Utah.Udot.ATSPM.ReportApi.ReportServices;

namespace Utah.Udot.ATSPM.ReportApi.Controllers
{
    public class WatchDogDashboardController : ControllerBase
    {
        private readonly WatchDogDashboardReportService watchDogDashboardReportService;

        public WatchDogDashboardController(WatchDogDashboardReportService watchDogDashboardReportService)
        {
            this.watchDogDashboardReportService = watchDogDashboardReportService;
        }

        [HttpPost("getIssueTypeGroup")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<WatchDogIssueTypeGroup> GetIssueTypeGroup([FromBody] WatchDogDashboardOptions options)
        {
            try
            {
                var result = watchDogDashboardReportService.GetDashboardGroup(options);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("getDetectionTypeGroup")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<WatchDogIssueTypeGroup> GetDetectionTypeGroup([FromBody] WatchDogDashboardOptions options)
        {
            try
            {
                var result = watchDogDashboardReportService.GetDetectionTypeGroups(options);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
