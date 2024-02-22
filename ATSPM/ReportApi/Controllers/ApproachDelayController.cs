using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.AppoachDelay;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Approach delay report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class ApproachDelayController : ReportControllerBase<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>
    {
        /// <inheritdoc/>
        public ApproachDelayController(IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>> reportService) : base(reportService) { }
    }
}