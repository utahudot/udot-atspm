using Microsoft.AspNetCore.Identity;

public class ClaimsService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public ClaimsService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IList<string>> GetAllClaimsForRole(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        var claims = await _roleManager.GetClaimsAsync(role);
        return claims.Select(c => c.Type).ToList();
    }

    public async Task<bool> AddClaimToRole(string roleName, string claimType, string claimValue)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        var result = await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(claimType, claimValue));
        return result.Succeeded;
    }

    public async Task<bool> RemoveClaimFromRole(string roleName, string claimType, string claimValue)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        var result = await _roleManager.RemoveClaimAsync(role, new System.Security.Claims.Claim(claimType, claimValue));
        return result.Succeeded;
    }
}
