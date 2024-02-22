using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.PreempDetail;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Preempt detail report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class PreemptDetailController : ReportControllerBase<PreemptDetailOptions, PreemptDetailResult>
    {
        /// <inheritdoc/>
        public PreemptDetailController(IReportService<PreemptDetailOptions, PreemptDetailResult> reportService) : base(reportService) { }
    }
}