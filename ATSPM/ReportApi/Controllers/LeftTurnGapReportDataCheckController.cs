using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.LeftTurnGapReport;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Left turn gap analysis report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class LeftTurnGapReportDataCheckController : ReportControllerBase<LeftTurnGapDataCheckOptions, LeftTurnGapDataCheckResult>
    {
        /// <inheritdoc/>
        public LeftTurnGapReportDataCheckController(IReportService<LeftTurnGapDataCheckOptions, LeftTurnGapDataCheckResult> reportService) : base(reportService) { }
    }
}