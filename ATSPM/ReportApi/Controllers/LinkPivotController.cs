using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.LinkPivot;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Left turn gap analysis report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class LinkPivotController : ReportControllerBase<LinkPivotOptions, IEnumerable<LinkPivotResult>>
    {
        /// <inheritdoc/>
        public LinkPivotController(IReportService<LinkPivotOptions, IEnumerable<LinkPivotResult>> reportService) : base(reportService) { }
    }
}