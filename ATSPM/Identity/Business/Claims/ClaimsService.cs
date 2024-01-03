using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Claims;

namespace ATSPM.Identity.Business.Claims
{
    public class ClaimsService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IdentityContext _context;

        public ClaimsService(RoleManager<IdentityRole> roleManager, IdentityContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IList<string>> GetAllClaimsForRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var claims = await _roleManager.GetClaimsAsync(role);
            return claims.Select(c => c.Value).ToList();
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

        public async Task AddClaimsToRole(string roleName, List<string> claims)
        {
            var existingRole = await _roleManager.FindByNameAsync(roleName);
            var existingClaims = _roleManager.GetClaimsAsync(existingRole).Result.Select(claim => claim.Value);

            var i1 = claims.Except(existingClaims).ToList();
            var i2 = existingClaims.Except(claims).ToList();

            var u = i1.Union(i2).ToList();
            foreach (var claimName in u)
            {
                var roleHasClaim = existingClaims.Any(c => c == claimName);
                var existingClaim = new Claim(ClaimTypes.Role, claimName);

                if (!roleHasClaim)
                {
                    // Add the existing claim to the role
                    await _roleManager.AddClaimAsync(existingRole, existingClaim);
                } else
                {
                    await _roleManager.RemoveClaimAsync(existingRole, existingClaim);
                }
                
            }
        }
    }
}