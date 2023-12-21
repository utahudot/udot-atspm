using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Watchdog;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Preempt request report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class WatchdogController : ReportControllerBase<WatchDogOptions, WatchDogResult>
    {
        /// <inheritdoc/>
        public WatchdogController(IReportService<WatchDogOptions, WatchDogResult> reportService) : base(reportService) { }
    }
}