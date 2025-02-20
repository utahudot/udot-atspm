#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration.Identity/RolesAndClaimsDBInitializer.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Utah.Udot.Atspm.Data.Configuration.Identity
{
    public class RolesAndClaimsDBInitializer
    {
        private readonly ILogger<RolesAndClaimsDBInitializer> _logger;

        public RolesAndClaimsDBInitializer(ILogger<RolesAndClaimsDBInitializer> logger)
        {
            _logger = logger;
        }
        public static async Task SeedRolesAndClaims(IServiceProvider serviceProvider, string connectionString)
        {
            // Seed roles
            using (var scope = serviceProvider.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<RolesAndClaimsDBInitializer>>();
                var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();

                // If a connection string is provided, update the context's connection string.
                if (!string.IsNullOrEmpty(connectionString))
                {
                    identityContext.Database.GetDbConnection().ConnectionString = connectionString;
                }

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                logger.LogInformation("Starting role seeding process.");

                if (!roleManager.Roles.Any())
                {
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
                            logger.LogInformation($"Role '{role}' created successfully.");
                        }
                        else
                        {
                            logger.LogInformation($"Role '{role}' already exists.");
                        }
                    }
                }
                else
                {
                    logger.LogInformation("Roles already exist. Skipping role creation.");
                }
            }

            // Seed claims for roles
            using (var claimScope = serviceProvider.CreateScope())
            {
                var logger = claimScope.ServiceProvider.GetRequiredService<ILogger<RolesAndClaimsDBInitializer>>();
                var identityContext = claimScope.ServiceProvider.GetRequiredService<IdentityContext>();

                // Update connection string if provided.
                if (!string.IsNullOrEmpty(connectionString))
                {
                    identityContext.Database.GetDbConnection().ConnectionString = connectionString;
                }

                var roleManager = claimScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Define claims for each role
                var claims = new Dictionary<string, List<Claim>>
        {
            { "UserAdmin", new List<Claim>
                {
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:View"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:Edit"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:Delete")
                }
            },
            { "Admin", new List<Claim>
                {
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin")
                }
            },
            { "RoleAdmin", new List<Claim>
                {
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:View"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:Edit"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:Delete")
                }
            },
            { "LocationConfigurationAdmin", new List<Claim>
                {
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:View"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:Edit"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:Delete")
                }
            },
            { "GeneralConfigurationAdmin", new List<Claim>
                {
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:View"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:Edit"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:Delete")
                }
            },
            { "DataAdmin", new List<Claim>
                {
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Data:View"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Data:Edit")
                }
            },
            { "WatchdogSubscriber", new List<Claim>
                {
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Watchdog:View"),
                    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Report:View")
                }
            }
        };

                // Loop through each role and add claims.
                foreach (var roleClaims in claims)
                {
                    var scopedRoleManager = claimScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var role = await scopedRoleManager.FindByNameAsync(roleClaims.Key);
                    if (role != null)
                    {
                        foreach (var claim in roleClaims.Value)
                        {
                            // Check if the claim already exists for the role.
                            var existingClaims = await scopedRoleManager.GetClaimsAsync(role);
                            if (!existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                            {
                                await scopedRoleManager.AddClaimAsync(role, claim);
                                logger.LogInformation($"Claim '{claim.Type}: {claim.Value}' added to role '{roleClaims.Key}'.");
                            }
                            else
                            {
                                logger.LogInformation($"Claim '{claim.Type}: {claim.Value}' already exists for role '{roleClaims.Key}'.");
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning($"Role '{roleClaims.Key}' not found while adding claims.");
                    }
                }

                logger.LogInformation("Role and claim seeding process completed.");
            }
        }

    }
}

