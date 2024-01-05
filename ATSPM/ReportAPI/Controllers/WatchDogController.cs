using Asp.Versioning;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Watchdog;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
        [HttpGet("GetIssueTypes")]
        public IEnumerable<WatchDogIssueTypeDTO> GetIssueTypes()
        {
            var issues = Enum.GetValues(typeof(WatchDogIssueType))
                .Cast<WatchDogIssueType>()
                .Select(e => new WatchDogIssueTypeDTO { Id = (int)e, Name = e.ToString() })
                .ToList();

            return issues;
        }
    }

}