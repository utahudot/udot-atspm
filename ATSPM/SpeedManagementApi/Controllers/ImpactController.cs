using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SpeedManagementApi.Controllers
{
    [Authorize]
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
        public async Task<ActionResult<Impact>> GetImpactById(Guid id)
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
            // Extract the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                Impact.CreatedBy = userId;
                Impact.CreatedOn = DateTime.UtcNow;
            }
            else
            {
                return BadRequest();
            }

            Impact impact = await impactService.UpsertImpact(Impact);
            return Ok(impact);
        }

        // PUT: /Impact/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImpact(Guid id, [FromBody] Impact Impact)
        {
            if (Impact == null || id != Impact.Id)
            {
                return BadRequest();
            }
            // Extract the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                Impact.UpdatedBy = userId;
                Impact.UpdatedOn = DateTime.UtcNow;
            }
            else
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
        public async Task<IActionResult> AddImpactedSegment(Guid id, Guid segmentId, [FromBody] Impact Impact)
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
        public async Task<IActionResult> DeleteImpact(Guid id)
        {
            var existingImpact = await impactService.GetImpactById(id);
            if (existingImpact == null)
            {
                return NotFound();
            }

            // Extract the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                var timeNow = DateTime.UtcNow;
                existingImpact.DeletedBy = userId;
                existingImpact.DeletedOn = timeNow;
                existingImpact.UpdatedBy = userId;
                existingImpact.UpdatedOn = timeNow;
            }
            else
            {
                return BadRequest();
            }

            await impactService.DeleteImpact(existingImpact);
            return NoContent();
        }

        // DELETE: /Impact/{id}/segment/{segment}
        [HttpDelete("{id}/segments/{segmentId}")]
        public async Task<IActionResult> DeleteImpactedSegment(Guid id, Guid segmentId)
        {
            Impact existingImpact = await impactService.GetImpactById(id);
            if (existingImpact == null)
            {
                return NotFound();
            }
            bool contains = existingImpact.Segments.Select(i => i.Id.Equals(segmentId)).Any();
            if (!contains)
            {
                return NotFound();
            }
            await impactService.DeleteImpactedSegment(id, segmentId);
            return NoContent();
        }

    }
}
