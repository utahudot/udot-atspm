using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ImpactTypeController : ControllerBase
    {
        private ImpactTypeService impactTypeService;

        public ImpactTypeController(ImpactTypeService impactTypeService)
        {
            this.impactTypeService = impactTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImpactType>>> ListImpactTypes()
        {
            var impactTypes = await impactTypeService.ListImpactTypes();
            return Ok(impactTypes);
        }

        // GET: /ImpactType/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ImpactType>> GetImpactTypeById(Guid id)
        {
            var impactType = await impactTypeService.GetImpactTypeById(id);
            if (impactType == null)
            {
                return NotFound();
            }
            return Ok(impactType);
        }

        // POST: /ImpactType
        [HttpPost]
        public async Task<ActionResult<ImpactType>> CreateImpactType([FromBody] ImpactType impactType)
        {
            if (impactType == null)
            {
                return BadRequest();
            }
            impactType.Id = null;
            var newImpactType = await impactTypeService.UpsertAsync(impactType);
            return Ok(newImpactType);
        }

        // PUT: /ImpactType/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ImpactType>> UpdateImpactType(Guid id, [FromBody] ImpactType impactType)
        {
            if (id == null || (impactType.Id != null && id != impactType.Id))
            {
                return BadRequest();
            }

            var existingImpactType = await impactTypeService.GetImpactTypeById(id);
            if (existingImpactType == null)
            {
                return NotFound();
            }
            impactType.Id = existingImpactType.Id;
            var newImpactType = await impactTypeService.UpsertAsync(impactType);
            return Ok(newImpactType);
        }

        // DELETE: /ImpactType/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImpactType(Guid id)
        {
            var existingImpactType = await impactTypeService.GetImpactTypeById(id);
            if (existingImpactType == null)
            {
                return NotFound();
            }

            await impactTypeService.DeleteAsync(existingImpactType);
            return NoContent();
        }

    }
}
