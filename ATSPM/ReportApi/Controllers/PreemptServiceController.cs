using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.PreemptService;
using ATSPM.Data.Models;
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