using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.LeftTurnGapAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Left turn gap analysis report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class LeftTurnGapAnalysisController : ReportControllerBase<LeftTurnGapAnalysisOptions, IEnumerable<LeftTurnGapAnalysisResult>>
    {
        /// <inheritdoc/>
        public LeftTurnGapAnalysisController(IReportService<LeftTurnGapAnalysisOptions, IEnumerable<LeftTurnGapAnalysisResult>> reportService) : base(reportService) { }
    }
}