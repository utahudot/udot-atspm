using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.SplitMonitor;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Split monitor report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class SplitMonitorController : ReportControllerBase<SplitMonitorOptions, IEnumerable<SplitMonitorResult>>
    {
        /// <inheritdoc/>
        public SplitMonitorController(IReportService<SplitMonitorOptions, IEnumerable<SplitMonitorResult>> reportService) : base(reportService) { }
    }
}