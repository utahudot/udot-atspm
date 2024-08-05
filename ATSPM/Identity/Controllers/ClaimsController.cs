using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Identity.Business.Claims;
using Identity.Models.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace Identity.Controllers
{
    [Authorize()]
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsController : ControllerBase
    {
        private readonly ClaimsService claimsService;

        public ClaimsController(ClaimsService claimsService)
        {
            this.claimsService = claimsService;
        }

        // GET: api/claims
        [HttpGet]
        [Authorize(Policy = "CanViewRoles")]
        public async Task<IActionResult> GetClaims()
        {
            var descriptions = Enum.GetValues(typeof(ClaimTypes))
                                   .Cast<Enum>()
                                   .Select(e => e.GetDisplayName())
                                   .ToList();

            return Ok(descriptions);
        }

        // GET: api/claims/roleName
        [HttpGet("{roleName}")]
        [Authorize(Policy = "CanViewRoles")]
        public async Task<IActionResult> GetClaimsForRole(string roleName)
        {
            return Ok(await claimsService.GetAllClaimsForRole(roleName));
        }

        // POST: api/claims/roleName
        [HttpPost("{roleName}")]
        [Authorize(Policy = "CanEditRoles")]
        public async Task<IActionResult> AddClaimToRole(string roleName, [FromBody] ClaimModel claim)
        {
            if (await claimsService.AddClaimToRole(roleName, claim.Type, claim.Value))
                return Ok();
            return BadRequest("Could not add claim to role.");
        }

        // POST: api/claims/addClaims/roleName
        [HttpPost("add/{roleName}")]
        [Authorize(Policy = "CanEditRoles")]
        public async Task<IActionResult> AddClaimsToRole(string roleName, [FromBody] ClaimsModel model)
        {
            try
            {
                await claimsService.AddClaimsToRole(roleName, model.Claims);
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // DELETE: api/claims/roleName
        [HttpDelete("{roleName}")]
        [Authorize(Policy = "CanEditRoles")]
        public async Task<IActionResult> RemoveClaimFromRole(string roleName, [FromBody] ClaimModel claim)
        {
            if (await claimsService.RemoveClaimFromRole(roleName, claim.Type, claim.Value))
                return Ok();
            return BadRequest("Could not remove claim from role.");
        }
    }
}