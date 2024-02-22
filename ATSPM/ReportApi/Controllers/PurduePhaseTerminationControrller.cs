using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.PhaseTermination;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Purdue phase termination report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class PurduePhaseTerminationController : ReportControllerBase<PurduePhaseTerminationOptions, PhaseTerminationResult>
    {
        /// <inheritdoc/>
        public PurduePhaseTerminationController(IReportService<PurduePhaseTerminationOptions, PhaseTerminationResult> reportService) : base(reportService) { }
    }
}