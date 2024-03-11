using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.Watchdog;
using ATSPM.Data.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
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
