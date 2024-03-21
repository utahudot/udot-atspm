using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.LinkPivot;
using ATSPM.ReportApi.ReportServices;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Left turn gap analysis report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class LinkPivotController : ReportControllerBase<LinkPivotOptions, LinkPivotResult>
    {
        private readonly LinkPivotReportService linkPivotReportService;
        /// <inheritdoc/>
        public LinkPivotController(IReportService<LinkPivotOptions, LinkPivotResult> reportService, LinkPivotReportService linkPivotReportService) : base(reportService) {
            this.linkPivotReportService = linkPivotReportService;
        }

        [HttpPost("getPcdData")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LinkPivotPcdResult>> GetPcdData(LinkPivotPcdOptions options)
        {
            try
            {
                var result = await linkPivotReportService.GetPcdData(options);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}