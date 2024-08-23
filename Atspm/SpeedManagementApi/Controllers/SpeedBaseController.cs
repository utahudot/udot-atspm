using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    public class SpeedBaseController<Tin, Tout> : ControllerBase
    {
        private IReportService<Tin, Tout> _reportService;

        public SpeedBaseController(IReportService<Tin, Tout> reportService)
        {
            _reportService = reportService;
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
