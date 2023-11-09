using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PreemptServiceRequest;
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