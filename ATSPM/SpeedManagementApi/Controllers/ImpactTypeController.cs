using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<ActionResult<ImpactType>> GetImpactTypeById(int id)
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
        public async Task<IActionResult> CreateImpactType([FromBody] ImpactType impactType)
        {
            if (impactType == null)
            {
                return BadRequest();
            }

            await impactTypeService.UpdateAsync(impactType);
            return NoContent();
        }

        // PUT: /ImpactType/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImpactType(int id, [FromBody] ImpactType impactType)
        {
            if (impactType == null || id != impactType.Id)
            {
                return BadRequest();
            }

            var existingImpactType = await impactTypeService.GetImpactTypeById(id);
            if (existingImpactType == null)
            {
                return NotFound();
            }

            await impactTypeService.UpdateAsync(impactType);
            return NoContent();
        }

        // DELETE: /ImpactType/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImpactType(int id)
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
