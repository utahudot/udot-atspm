﻿using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ATSPM.Infrastructure.Migrations.Identity
{
    public static class ConfigurationSeedData
    {
        // Methods to define seed data
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
        {
            new ApiResource("reportsApi", "Reports API")
            {
                Scopes = { "reports.public", "reports.private" }
            },

            new ApiResource("dataApi", "Data API")
            {
                Scopes = { "data.access" }
            },

            new ApiResource("configApi", "Configuration API")
            {
                Scopes = { "config.public", "config.admin" }
            },

            new ApiResource("identityApi", "Identity API")
            {
                Scopes = { "config.public", "config.admin" }
            },

            // If the Admin Utility interacts with an API, it could also be represented as an ApiResource.
            // If it's just an executable without a backend API, then it may not need an ApiResource representation.
            new ApiResource("adminUtilityApi", "Admin Utility API")
            {
                Scopes = { "admin.utility" }
            }
        };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
        {
            new ApiScope("reports.private", "Access to private reports"),
            new ApiScope("reports.public", "Access to public reports"),
            new ApiScope("data.access", "Access to data API"),
            new ApiScope("config.admin", "Access to admin configuration"),
            new ApiScope("admin.utility", "Access to admin utility"),
            new ApiScope("config.public", "Basic user access")
        };
        }

        // ... other parts of ConfigurationSeedData ...

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                //new IdentityResource(
                //    name: "openid",
                //    userClaims: new[] { "sub" },
                //    displayName: "Your user identifier"),
                //new IdentityResource(
                //    name: "profile",
                //    userClaims: new[] { "name", "email", "website" },
                //    displayName: "Your profile data")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
    {
         new Client
        {
            ClientId = "ATSPMWeb",
             ClientSecrets =
            {
                new Secret("ATSPMWebTest".ToSha256())
            },
            AllowedScopes = {
                "reports.public",
                "reports.private",
                "data.access",
                "config.admin"
                // ... other scopes ...
            }
        },
        new Client
        {
            ClientId = "EventLogUtility",
            ClientSecrets =
            {
                new Secret("ATSPMEventLogUtilityTest".ToSha256())
            },
            AllowedScopes = { "admin.utility" }
        },
        new Client
        {
            ClientId = "PostmanTest",
            ClientSecrets =
            {
                new Secret("PostmanTest".ToSha256())
            },
            RedirectUris = { "https://127.0.0.1:44357" },
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes = { "reports.public","reports.private","config.admin","config.public","admin.utility" },
            RequireConsent = false,
            RequirePkce = false,
        },
            new Client
        {
            ClientId = "Identity",
                ClientSecrets =
            {
                new Secret("IdentityTest".ToSha256())
            },
            AllowedScopes = { "config.admin", "config.public" }
        },
    };
        }

        public static void Seed(IdentityConfigurationContext context)
        {
            // ... seeding logic for ApiResources and ApiScopes ...

            // Seed IdentityResources
            if (!context.IdentityResources.Any())
            {
                foreach (var resource in GetIdentityResources())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            // Seed Clients
            if (!context.Clients.Any())
            {
                foreach (var client in GetClients())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            // Seed ApiResources
            if (!context.ApiResources.Any())
            {
                foreach (var resource in GetApiResources())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            // Seed ApiScopes
            if (!context.ApiScopes.Any())
            {
                foreach (var scope in GetApiScopes())
                {
                    context.ApiScopes.Add(scope.ToEntity());
                }
                context.SaveChanges();
            }

            // ... similar logic for seeding clients, identity resources, etc. ...
        }

        private static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            // Define your roles
            string[] roles = new string[]
            {
        "Admin",
        "User",
                // Add other roles as needed
            };

            foreach (var roleName in roles)
            {
                // Check if the role already exists
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    // Create the role
                    var role = new IdentityRole(roleName);
                    var result = roleManager.CreateAsync(role).Result;

                    if (result.Succeeded)
                    {
                        // Add claims to the role
                        AddClaimsToRole(roleManager, roleName);
                    }
                    else
                    {
                        // Handle role creation failure
                    }
                }
            }
        }

        private static void AddClaimsToRole(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var claims = CustomClaims.GetAllClaimsFromCustomClaims();

            var role = roleManager.FindByNameAsync(roleName).Result;
            if (role != null)
            {
                foreach (var claim in claims)
                {
                    roleManager.AddClaimAsync(role, new Claim(ClaimTypes.Role, claim)).Wait();
                }
            }
        }



    }
}