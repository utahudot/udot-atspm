using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.TurningMovementCounts;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Turning movement count report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class TurningMovementCountsController : ReportControllerBase<TurningMovementCountsOptions, IEnumerable<TurningMovementCountsResult>>
    {
        /// <inheritdoc/>
        public TurningMovementCountsController(IReportService<TurningMovementCountsOptions, IEnumerable<TurningMovementCountsResult>> reportService) : base(reportService) { }
    }
}