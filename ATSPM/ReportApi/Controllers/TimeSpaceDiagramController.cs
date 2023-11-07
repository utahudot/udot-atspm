using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.TimeSpaceDiagram;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Time space diagram report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class TimeSpaceDiagramController : ReportControllerBase<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResults>>
    {
        /// <inheritdoc/>
        public TimeSpaceDiagramController(IReportService<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResults>> reportService) : base(reportService) { }
    }
}