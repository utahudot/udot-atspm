using Asp.Versioning;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.YellowRedActivations;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Yellow and red activations report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class YellowRedActivationsController : ReportControllerBase<YellowRedActivationsOptions, IEnumerable<YellowRedActivationsResult>>
    {
        /// <inheritdoc/>
        public YellowRedActivationsController(IReportService<YellowRedActivationsOptions, IEnumerable<YellowRedActivationsResult>> reportService) : base(reportService) { }
    }
}