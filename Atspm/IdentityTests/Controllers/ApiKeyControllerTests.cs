#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityTests - Utah.Udot.Atspm.IdentityTests.Controllers/ApiKeyControllerTests.cs
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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.ATSPM.IdentityApi.Controllers;
using Utah.Udot.ATSPM.IdentityApi.Dto;
using Xunit;

namespace Utah.Udot.Atspm.IdentityTests.Controllers
{
    public class ApiKeyControllerTests
    {
        [Theory]
        [InlineData(AtspmAuthorization.Permissions.Admin)]
        [InlineData(AtspmAuthorization.Permissions.ApiKeysCreate)]
        public async Task Create_WithForbiddenClaim_ReturnsBadRequest(string claim)
        {
            using var context = CreateContext();
            var controller = CreateController(context, "admin-user", new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Roles.Admin)
            });

            var result = await controller.Create(new CreateApiKeyDto
            {
                Name = "Forbidden key",
                Claims = new List<string> { claim }
            });

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Empty(context.ApiKeys);
        }

        [Fact]
        public async Task Create_WithUnknownClaim_ReturnsBadRequest()
        {
            using var context = CreateContext();
            var controller = CreateController(context, "admin-user", new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Roles.Admin)
            });

            var result = await controller.Create(new CreateApiKeyDto
            {
                Name = "Unknown claim key",
                Claims = new List<string> { "Unknown:Permission" }
            });

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Empty(context.ApiKeys);
        }

        [Fact]
        public async Task Create_WithClaimUserDoesNotHave_ReturnsForbidden()
        {
            using var context = CreateContext();
            var controller = CreateController(context, "standard-user", new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Permissions.DataView)
            });

            var result = await controller.Create(new CreateApiKeyDto
            {
                Name = "Escalated key",
                Claims = new List<string> { AtspmAuthorization.Permissions.DataEdit }
            });

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
            Assert.Empty(context.ApiKeys);
        }

        [Fact]
        public async Task GetMyKeys_ForNonAdmin_ReturnsOnlyOwnedKeysIncludingRevoked()
        {
            using var context = CreateContext();
            SeedUsers(context);
            SeedApiKeys(context);

            var controller = CreateController(context, "user-1", new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Permissions.ApiKeysView)
            });

            var result = await controller.GetMyKeys();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var keys = Assert.IsAssignableFrom<List<ApiKeyMetadataDto>>(okResult.Value);
            Assert.Equal(2, keys.Count);
            Assert.All(keys, key => Assert.Equal("user-1", key.OwnerId));
            Assert.Contains(keys, key => key.IsRevoked);
        }

        [Fact]
        public async Task GetMyKeys_ForGlobalAdmin_ReturnsAllKeysIncludingRevoked()
        {
            using var context = CreateContext();
            SeedUsers(context);
            SeedApiKeys(context);

            var controller = CreateController(context, "admin-user", new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Roles.Admin)
            });

            var result = await controller.GetMyKeys();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var keys = Assert.IsAssignableFrom<List<ApiKeyMetadataDto>>(okResult.Value);
            Assert.Equal(3, keys.Count);
            Assert.Contains(keys, key => key.OwnerId == "user-2");
            Assert.Contains(keys, key => key.IsRevoked);
        }

        [Fact]
        public async Task Revoke_ForGlobalAdmin_CanRevokeAnotherUsersKey()
        {
            using var context = CreateContext();
            SeedUsers(context);
            SeedApiKeys(context);

            var controller = CreateController(context, "admin-user", new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Roles.Admin)
            });

            var result = await controller.Revoke(3);

            Assert.IsType<OkObjectResult>(result);
            Assert.True(await context.ApiKeys.Where(k => k.Id == 3).Select(k => k.IsRevoked).SingleAsync());
        }

        [Fact]
        public async Task Revoke_ForNonAdmin_ReturnsNotFoundForAnotherUsersKey()
        {
            using var context = CreateContext();
            SeedUsers(context);
            SeedApiKeys(context);

            var controller = CreateController(context, "user-1", new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Permissions.ApiKeysRevoke)
            });

            var result = await controller.Revoke(3);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.False(await context.ApiKeys.Where(k => k.Id == 3).Select(k => k.IsRevoked).SingleAsync());
        }

        [Fact]
        public async Task Authorization_WithApiKeyAdminClaim_DoesNotUseAdminBypass()
        {
            using var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddAtspmAuthorization()
                .BuildServiceProvider();
            var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
            var policyName = AtspmAuthorization.GetPolicyName(AtspmAuthorization.Permissions.DataView);

            var apiKeyPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Roles.Admin),
                new Claim("AuthenticationMethod", "ApiKey")
            }, "ApiKey"));
            var jwtPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, AtspmAuthorization.Roles.Admin)
            }, JwtBearerDefaults.AuthenticationScheme));

            var apiKeyResult = await authorizationService.AuthorizeAsync(apiKeyPrincipal, null, policyName);
            var jwtResult = await authorizationService.AuthorizeAsync(jwtPrincipal, null, policyName);

            Assert.False(apiKeyResult.Succeeded);
            Assert.True(jwtResult.Succeeded);
        }

        private static IdentityContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new IdentityContext(options);
        }

        private static ApiKeyController CreateController(
            IdentityContext context,
            string userId,
            IEnumerable<Claim> extraClaims)
        {
            var userManager = CreateUserManager();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            claims.AddRange(extraClaims);

            var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);

            return new ApiKeyController(context, userManager)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(identity)
                    }
                }
            };
        }

        private static UserManager<ApplicationUser> CreateUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new UserManager<ApplicationUser>(
                store.Object,
                Options.Create(new IdentityOptions()),
                Mock.Of<IPasswordHasher<ApplicationUser>>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                Mock.Of<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Mock.Of<IServiceProvider>(),
                NullLogger<UserManager<ApplicationUser>>.Instance);
        }

        private static void SeedUsers(IdentityContext context)
        {
            context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "user-1",
                    Email = "one@example.com",
                    FirstName = "User",
                    LastName = "One"
                },
                new ApplicationUser
                {
                    Id = "user-2",
                    Email = "two@example.com",
                    FirstName = "User",
                    LastName = "Two"
                });

            context.SaveChanges();
        }

        private static void SeedApiKeys(IdentityContext context)
        {
            context.ApiKeys.AddRange(
                new ApiKey
                {
                    Id = 1,
                    Name = "Active owned key",
                    KeyHash = "hash-1",
                    OwnerId = "user-1",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                    IsRevoked = false,
                    Claims = new List<ApiKeyClaim>
                    {
                        new ApiKeyClaim
                        {
                            Type = ClaimTypes.Role,
                            Value = AtspmAuthorization.Permissions.DataView
                        }
                    }
                },
                new ApiKey
                {
                    Id = 2,
                    Name = "Revoked owned key",
                    KeyHash = "hash-2",
                    OwnerId = "user-1",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                    IsRevoked = true,
                    Claims = new List<ApiKeyClaim>
                    {
                        new ApiKeyClaim
                        {
                            Type = ClaimTypes.Role,
                            Value = AtspmAuthorization.Permissions.ReportView
                        }
                    }
                },
                new ApiKey
                {
                    Id = 3,
                    Name = "Other user key",
                    KeyHash = "hash-3",
                    OwnerId = "user-2",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                    IsRevoked = false,
                    Claims = new List<ApiKeyClaim>
                    {
                        new ApiKeyClaim
                        {
                            Type = ClaimTypes.Role,
                            Value = AtspmAuthorization.Permissions.DataView
                        }
                    }
                });

            context.SaveChanges();
        }
    }
}
