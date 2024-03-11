using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.TimeSpaceDiagram;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Time space diagram report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class TimeSpaceDiagramAverageController : ReportControllerBase<TimeSpaceDiagramAverageOptions, IEnumerable<TimeSpaceDiagramAverageResult>>
    {
        /// <inheritdoc/>
        public TimeSpaceDiagramAverageController(IReportService<TimeSpaceDiagramAverageOptions, IEnumerable<TimeSpaceDiagramAverageResult>> reportService) : base(reportService) { }
    }
}
