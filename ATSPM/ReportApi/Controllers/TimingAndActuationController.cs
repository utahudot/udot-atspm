using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.TimingAndActuation;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Timing and actuation report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class TimingAndActuationController : ReportControllerBase<TimingAndActuationsOptions, IEnumerable<TimingAndActuationsForPhaseResult>>
    {
        /// <inheritdoc/>
        public TimingAndActuationController(IReportService<TimingAndActuationsOptions, IEnumerable<TimingAndActuationsForPhaseResult>> reportService) : base(reportService) { }
    }
}