
using ATSPM.Identity.Business.Claims;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ClaimsController : ControllerBase
{
    private readonly ClaimsService _claimsService;

    public ClaimsController(ClaimsService claimsService)
    {
        _claimsService = claimsService;
    }

    // GET: api/claims/roleName
    [HttpGet("{roleName}")]
    public async Task<IActionResult> GetClaimsForRole(string roleName)
    {
        return Ok(await _claimsService.GetAllClaimsForRole(roleName));
    }

    // POST: api/claims/roleName
    [HttpPost("{roleName}")]
    public async Task<IActionResult> AddClaimToRole(string roleName, [FromBody] ClaimModel claim)
    {
        if (await _claimsService.AddClaimToRole(roleName, claim.Type, claim.Value))
            return Ok();
        return BadRequest("Could not add claim to role.");
    }

    // DELETE: api/claims/roleName
    [HttpDelete("{roleName}")]
    public async Task<IActionResult> RemoveClaimFromRole(string roleName, [FromBody] ClaimModel claim)
    {
        if (await _claimsService.RemoveClaimFromRole(roleName, claim.Type, claim.Value))
            return Ok();
        return BadRequest("Could not remove claim from role.");
    }
}

public class ClaimModel
{
    public string Type { get; set; }
    public string Value { get; set; }
}