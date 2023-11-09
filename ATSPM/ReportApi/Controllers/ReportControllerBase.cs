using ATSPM.ReportApi.Business;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.Controllers
{
    /// <summary>
    /// Report controller base
    /// </summary>
    /// <typeparam name="Tin">Input options</typeparam>
    /// <typeparam name="Tout">Output results</typeparam>
    public abstract class ReportControllerBase<Tin, Tout> : ControllerBase
    {
        private IReportService<Tin, Tout> _reportService;

        /// <inheritdoc/>
        public ReportControllerBase(IReportService<Tin, Tout> reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Get example data for testing
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual ActionResult<Tout> Test()
        {
            return Ok(new Fixture().Create<Tout>());
        }

        /// <summary>
        /// Get report data
        /// </summary>
        /// <param name="options">Reporting options</param>
        /// <returns></returns>
        [HttpPost("getReportData")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<ActionResult<Tout>> GetReportData([FromBody] Tin options)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _reportService.ExecuteAsync(options, null, HttpContext.RequestAborted);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
