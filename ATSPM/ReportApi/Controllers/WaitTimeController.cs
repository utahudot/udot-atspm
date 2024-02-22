using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.Application.Business;
using ATSPM.Application.Business.WaitTime;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Wait time report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class WaitTimeController : ReportControllerBase<WaitTimeOptions, IEnumerable<WaitTimeResult>>
    {
        /// <inheritdoc/>
        public WaitTimeController(IReportService<WaitTimeOptions, IEnumerable<WaitTimeResult>> reportService) : base(reportService) { }
    }
}