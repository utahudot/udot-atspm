using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.ApproachSpeed;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Approach speed report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class ApproachSpeedController : ReportControllerBase<ApproachSpeedOptions, IEnumerable<ApproachSpeedResult>>
    {
        /// <inheritdoc/>
        public ApproachSpeedController(IReportService<ApproachSpeedOptions, IEnumerable<ApproachSpeedResult>> reportService) : base(reportService) { }
    }
}