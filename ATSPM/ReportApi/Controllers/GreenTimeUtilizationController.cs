using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.GreenTimeUtilization;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Green time utilization report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class GreenTimeUtilizationController : ReportControllerBase<GreenTimeUtilizationOptions, IEnumerable<GreenTimeUtilizationResult>>
    {
        /// <inheritdoc/>
        public GreenTimeUtilizationController(IReportService<GreenTimeUtilizationOptions, IEnumerable<GreenTimeUtilizationResult>> reportService) : base(reportService) { }
    }
}