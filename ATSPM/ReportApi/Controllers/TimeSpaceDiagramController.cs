using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.TimeSpaceDiagram;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Time space diagram report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class TimeSpaceDiagramController : ReportControllerBase<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResult>>
    {
        /// <inheritdoc/>
        public TimeSpaceDiagramController(IReportService<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResult>> reportService) : base(reportService) { }
    }
}