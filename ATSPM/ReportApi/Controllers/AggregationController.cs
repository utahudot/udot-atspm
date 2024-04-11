using Asp.Versioning;
using ATSPM.Application.Business;
using ATSPM.Application.Business.Aggregation;
using ATSPM.Data.Models;
using ATSPM.ReportApi.DataAggregation;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Approach delay report controller
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class AggregationController : ReportControllerBase<AggregationOptions, IEnumerable<AggregationResult>>
    {
        /// <inheritdoc/>
        public AggregationController(IReportService<AggregationOptions, IEnumerable<AggregationResult>> reportService) : base(reportService) { }

    }
}