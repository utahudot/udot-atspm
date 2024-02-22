using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.SplitFail;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Split fail report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class SplitFailController : ReportControllerBase<SplitFailOptions, IEnumerable<SplitFailsResult>>
    {
        /// <inheritdoc/>
        public SplitFailController(IReportService<SplitFailOptions, IEnumerable<SplitFailsResult>> reportService) : base(reportService) { }
    }
}