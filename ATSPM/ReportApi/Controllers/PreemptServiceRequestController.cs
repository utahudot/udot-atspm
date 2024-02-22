using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.PreemptServiceRequest;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Preempt request report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class PreemptServiceRequestController : ReportControllerBase<PreemptServiceRequestOptions, PreemptServiceRequestResult>
    {
        /// <inheritdoc/>
        public PreemptServiceRequestController(IReportService<PreemptServiceRequestOptions, PreemptServiceRequestResult> reportService) : base(reportService) { }
    }
}