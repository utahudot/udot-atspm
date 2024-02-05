
using ATSPM.Identity.Business.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize()]
[Route("api/[controller]")]
[ApiController]
public class ClaimsController : ControllerBase
{
    private readonly ClaimsService _claimsService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ClaimsController(ClaimsService claimsService, RoleManager<IdentityRole> roleManager)
    {
        _claimsService = claimsService;
        _roleManager = roleManager;
    }

    // GET: api/claims
    [HttpGet]
    public async Task<IActionResult> GetClaims()
    {
        var roles = _roleManager.Roles.ToList();
        var claims = new List<string>();
        foreach (var role in roles)
        {
            var roleClaims = await _claimsService.GetAllClaimsForRole(role.Name);
            claims = claims.Concat(roleClaims).ToList();
        }
        return Ok(new ClaimsModel() { Claims = new HashSet<string>(claims).ToList() });
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

    // POST: api/claims/addClaims/roleName
    [HttpPost("add/{roleName}")]
    public async Task<IActionResult> AddClaimsToRole(string roleName, [FromBody] ClaimsModel model)
    {
        try
        {
            await _claimsService.AddClaimsToRole(roleName, model.Claims);
            return Ok();

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        //return BadRequest("Could not add claim to role.");
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


public class ClaimsModel
{
    public List<string> Claims { get; set; }
}