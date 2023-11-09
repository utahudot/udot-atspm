using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PurdueCoordinationDiagram;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Purdue coordination diagram report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class PurdueCoordinationDiagramController : ReportControllerBase<PurdueCoordinationDiagramOptions, IEnumerable<PurdueCoordinationDiagramResult>>
    {
        /// <inheritdoc/>
        public PurdueCoordinationDiagramController(IReportService<PurdueCoordinationDiagramOptions, IEnumerable<PurdueCoordinationDiagramResult>> reportService) : base(reportService) { }
    }
}