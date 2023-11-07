using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.ArrivalOnRed;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Arrival on red report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class ArrivalOnRedController : ReportControllerBase<ArrivalOnRedOptions, IEnumerable<ArrivalOnRedResult>>
    {
        /// <inheritdoc/>
        public ArrivalOnRedController(IReportService<ArrivalOnRedOptions, IEnumerable<ArrivalOnRedResult>> reportService) : base(reportService) { }
    }
}