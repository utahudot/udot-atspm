using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SpeedManagementApi.Controllers
{
    [Authorize()]
    [Route("api/[controller]")]
    [ApiController]
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
            var impacts = await impactService.ListImpacts();
            return Ok(impacts);
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
        public async Task<ActionResult<Impact>> CreateImpact([FromBody] Impact impactObject)
        {
            if (impactObject == null)
            {
                return BadRequest();
            }
            impactObject.Id = null;
            // Extract the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                impactObject.CreatedBy = userId;
                impactObject.CreatedOn = DateTime.UtcNow;
            }
            else
            {
                return BadRequest();
            }

            Impact impact = await impactService.UpsertImpact(impactObject);
            return Ok(impact);
        }

        // PUT: /Impact/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Impact>> UpdateImpact(Guid id, [FromBody] Impact impactObject)
        {
            if (impactObject == null)
            {
                return BadRequest();
            }

            if (id == null || (impactObject.Id != null && id != impactObject.Id))
            {
                return BadRequest();
            }
            // Extract the user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                impactObject.UpdatedBy = userId;
                impactObject.UpdatedOn = DateTime.UtcNow;
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
            impactObject.Id = existingImpact.Id;
            Impact impact = await impactService.UpsertImpact(impactObject);
            return Ok(impact);
        }

        // PUT: /Impact/{id/segment/{segment}
        [HttpPut("{id}/segments/{segmentId}")]
        public async Task<ActionResult<Impact>> AddImpactedSegment(Guid id, Guid segmentId)
        {
            if (id == null)
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
            bool contains = existingImpact.SegmentIds.Select(i => i.Equals(segmentId)).Any();
            if (!contains)
            {
                return NotFound();
            }
            await impactService.DeleteImpactedSegment(id, segmentId);
            return NoContent();
        }

    }
}
