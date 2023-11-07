using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.AppoachDelay;
using ATSPM.ReportApi.Business.ApproachVolume;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Approach volume report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class ApproachVolumeController : ReportControllerBase<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>>
    {
        /// <inheritdoc/>
        public ApproachVolumeController(IReportService<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>> reportService) : base(reportService) { }
    }
}