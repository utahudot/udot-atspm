using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.PedDelay;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Ped delay report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class PedDelayController : ReportControllerBase<PedDelayOptions, IEnumerable<PedDelayResult>>
    {
        /// <inheritdoc/>
        public PedDelayController(IReportService<PedDelayOptions, IEnumerable<PedDelayResult>> reportService) : base(reportService) { }
    }
}