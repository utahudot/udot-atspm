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
            IReadOnlyList<Impact> Impacts = await impactService.ListImpacts();
            return Ok(Impacts);
        }

        // GET: /Impact/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Impact>> GetImpactById(int id)
        {
            Impact impact = await impactService.GetImpactById(id);
            if (impact == null)
            {
                return NotFound();
            }
            return Ok(impact);
        }

        // POST: /Impact
        [HttpPost]
        public async Task<IActionResult> CreateImpact([FromBody] Impact Impact)
        {
            if (Impact == null)
            {
                return BadRequest();
            }

            Impact impact = await impactService.UpsertImpact(Impact);
            return Ok(impact);
        }

        // PUT: /Impact/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImpact(int id, [FromBody] Impact Impact)
        {
            if (Impact == null || id != Impact.Id)
            {
                return BadRequest();
            }

            var existingImpact = await impactService.GetImpactById(id);
            if (existingImpact == null)
            {
                return NotFound();
            }

            Impact impact = await impactService.UpsertImpact(Impact);
            return Ok(impact);
        }

        // PUT: /Impact/{id/segment/{segment}
        [HttpPut("{id}/segments/{segmentId}")]
        public async Task<IActionResult> AddImpactedSegment(int id, int segmentId, [FromBody] Impact Impact)
        {
            if (Impact == null || id != Impact.Id)
            {
                return BadRequest();
            }

            var existingImpact = await impactService.GetImpactById(id);
            if (existingImpact == null)
            {
                return NotFound();
            }

            Impact impact = await impactService.UpsertImpactedSegment(id, segmentId);
            return Ok(impact);
        }

        // DELETE: /Impact/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImpact(int id)
        {
            var existingImpact = await impactService.GetImpactById(id);
            if (existingImpact == null)
            {
                return NotFound();
            }

            await impactService.DeleteImpact(existingImpact);
            return NoContent();
        }

        // DELETE: /Impact/{id}/segment/{segment}
        [HttpDelete("{id}/segments/{segmentId}")]
        public async Task<IActionResult> DeleteImpactedSegment(int id, int segmentId)
        {
            Impact existingImpact = await impactService.GetImpactById(id);
            if (existingImpact == null)
            {
                return NotFound();
            }
            bool contains = existingImpact.Segments.Select(i => i.Id == segmentId).Any();
            if (!contains)
            {
                return NotFound();
            }
            await impactService.DeleteImpactedSegment(id, segmentId);
            return NoContent();
        }

    }
}
