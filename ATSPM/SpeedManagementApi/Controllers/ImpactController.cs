using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImpactController : ControllerBase
    {
        private ImpactService impactService;

        public ImpactController(ImpactService impactService)
        {
            this.impactService = impactService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Impact>>> ListImpacts()
        {
            var Impacts = await impactService.ListImpacts();
            return Ok(Impacts);
        }

        // GET: /Impact/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Impact>> GetImpactById(int id)
        {
            var Impact = await impactService.GetByIdAsync(id);
            if (Impact == null)
            {
                return NotFound();
            }
            return Ok(Impact);
        }

        // POST: /Impact
        [HttpPost]
        public async Task<IActionResult> CreateImpact([FromBody] Impact Impact)
        {
            if (Impact == null)
            {
                return BadRequest();
            }

            await impactService.UpdateAsync(Impact);
            return NoContent();
        }

        // PUT: /Impact/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImpact(int id, [FromBody] Impact Impact)
        {
            if (Impact == null || id != Impact.Id)
            {
                return BadRequest();
            }

            var existingImpact = await impactService.GetByIdAsync(id);
            if (existingImpact == null)
            {
                return NotFound();
            }

            await impactService.UpdateAsync(Impact);
            return NoContent();
        }

        // DELETE: /Impact/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImpact(int id)
        {
            var existingImpact = await impactService.GetByIdAsync(id);
            if (existingImpact == null)
            {
                return NotFound();
            }

            await impactService.DeleteAsync(existingImpact);
            return NoContent();
        }

        // DELETE: /Impact/{id}
        [HttpDelete("{id}/{segment}")]
        public async Task<IActionResult> DeleteImpactedSegment(int id, int segment)
        {
            var existingImpact = await impactService.GetByIdAsync(id);
            if (existingImpact == null)
            {
                return NotFound();
            }

            await impactService.DeleteAsync(existingImpact);
            return NoContent();
        }

    }
}
