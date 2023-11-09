using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PreemptService;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Preempt service report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class PreemptServiceController : ReportControllerBase<PreemptServiceOptions, PreemptServiceResult>
    {
        /// <inheritdoc/>
        public PreemptServiceController(IReportService<PreemptServiceOptions, PreemptServiceResult> reportService) : base(reportService) { }
    }
}