using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.ApproachVolume;
using ATSPM.Data.Models;
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