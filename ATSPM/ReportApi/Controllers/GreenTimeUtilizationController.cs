using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.GreenTimeUtilization;
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