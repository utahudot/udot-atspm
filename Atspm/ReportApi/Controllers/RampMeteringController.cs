using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Business.RampMetering;
using Utah.Udot.Atspm.ReportApi.Controllers;

namespace Utah.Udot.ATSPM.ReportApi.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class RampMeteringController : ReportControllerBase<RampMeteringOptions, RampMeteringResult>
    {
        public RampMeteringController(IReportService<RampMeteringOptions, RampMeteringResult> reportService, ILogger<RampMeteringController> logger) : base(reportService, logger)
        {
        }
    }
}
