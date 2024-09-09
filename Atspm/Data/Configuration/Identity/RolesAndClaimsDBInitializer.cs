using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Utah.Udot.Atspm.Data.Configuration.Identity
{
    public class RolesAndClaimsDBInitializer
    {
        public static async Task SeedRolesAndClaims(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (roleManager.Roles.Any())
                {
                    return;
                }

                // Create roles
                var roles = new List<string>
            {
                "Admin", "UserAdmin", "RoleAdmin",
                "LocationConfigurationAdmin", "GeneralConfigurationAdmin",
                "DataAdmin", "WatchdogSubscriber", "ReportAdmin"
            };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Setup claims
                var claims = new Dictionary<string, List<Claim>>
            {
                {"UserAdmin", new List<Claim>
                    {
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:View"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:Edit"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:Delete")
                    }
                },
                {"Admin", new List<Claim>
                    {
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin")
                    }
                },
                {"RoleAdmin", new List<Claim>
                    {
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:View"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:Edit"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:Delete")
                    }
                },
                {"LocationConfigurationAdmin", new List<Claim>
                    {
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:View"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:Edit"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:Delete")
                    }
                },
                {"GeneralConfigurationAdmin", new List<Claim>
                    {
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:View"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:Edit"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:Delete")
                    }
                },
                {"DataAdmin", new List<Claim>
                    {
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Data:View"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Data:Edit")
                    }
                },
                {"WatchdogSubscriber", new List<Claim>
                    {
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Watchdog:View"),
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Report:View")
                    }
                }
            };

                foreach (var roleClaims in claims)
                {
                    var role = await roleManager.FindByNameAsync(roleClaims.Key);
                    foreach (var claim in roleClaims.Value)
                    {
                        var existingClaims = await roleManager.GetClaimsAsync(role);

                        if (!existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                        {
                            await roleManager.AddClaimAsync(role, claim);
                        }
                    }
                }
            }
        }




    }
}
