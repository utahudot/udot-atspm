#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.Infrastructure.Services.Identity.Tests/FederatedAuthServiceTests.cs
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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Infrastructure.Services.Identity;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Services.Identity.Dto;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.Identity.Tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="FederatedAuthService"/> class.
    /// </summary>
    public class FederatedAuthServiceTests
    {
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IUserGeographyRepository> _geographyRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<FederatedAuthService>> _loggerMock;
        private readonly FederatedAuthenticationConfiguration _fedConfig;
        private readonly FederatedAuthService _federatedAuthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FederatedAuthServiceTests"/> class and boots up standard mocks.
        /// </summary>
        public FederatedAuthServiceTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var loggerSignInMock = new Mock<ILogger<SignInManager<ApplicationUser>>>();
            var confirmationMock = new Mock<IUserConfirmation<ApplicationUser>>();

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                optionsMock.Object,
                loggerSignInMock.Object,
                null,
                confirmationMock.Object);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

            _geographyRepositoryMock = new Mock<IUserGeographyRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<FederatedAuthService>>();

            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-32-chars-long-12345");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("AtspmIssuer");
            _configurationMock.Setup(c => c["Jwt:ExpireDays"]).Returns("7");

            _fedConfig = new FederatedAuthenticationConfiguration();
            var provider = new FederatedProviderConfiguration
            {
                ProviderName = "Authentik",
                Authority = "https://authentik.agency.gov/application/o/atspm/",
                ClientId = "atspm-core-api",
                ClientSecret = "SUPER_SECRET_KEY",
                CallbackPath = "/signin-oidc",
                UserProfileClaims = new UserProfileClaimConfiguration
                {
                    Email = "email",
                    FirstName = "given_name",
                    LastName = "family_name",
                    Agency = "agency",
                    AreaIds = "atspm_areas",
                    RegionIds = "atspm_regions",
                    JurisdictionIds = "atspm_jurisdictions",
                    Roles = "roles"
                }
            };
            _fedConfig.Providers.Add(provider);

            var optionsFedMock = Options.Create(_fedConfig);

            _federatedAuthService = new FederatedAuthService(
                _signInManagerMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _geographyRepositoryMock.Object,
                optionsFedMock,
                _configurationMock.Object,
                _loggerMock.Object);
        }

        /// <summary>
        /// Verifies that <see cref="FederatedAuthService.PrepareChallengeAsync"/> successfully builds challenge parameters.
        /// </summary>
        [Fact]
        public async Task PrepareChallengeAsync_ShouldReturnChallengeProperties()
        {
            var providerName = "Authentik";
            var redirectUri = "/signin-oidc";
            var authProperties = new AuthenticationProperties();
            authProperties.Items["test-key"] = "test-value";

            _signInManagerMock.Setup(s => s.ConfigureExternalAuthenticationProperties(providerName, redirectUri, null))
                .Returns(authProperties);

            var result = await _federatedAuthService.PrepareChallengeAsync(providerName, redirectUri);

            Assert.NotNull(result);
            Assert.Equal(providerName, result.Scheme);
            Assert.Equal(redirectUri, result.RedirectUri);
            Assert.Contains("test-key", result.Properties.Keys);
            Assert.Equal("test-value", result.Properties["test-key"]);
        }

        /// <summary>
        /// Asserts that <see cref="FederatedAuthService.LinkAccountAsync"/> links external credentials.
        /// </summary>
        [Fact]
        public async Task LinkAccountAsync_ShouldAssociateExternalSsoWithUser()
        {
            var userId = "user-123";
            var externalInfo = new ExternalIdentityDto("Authentik", "authentik-key-abc", new Dictionary<string, string>());
            var user = new ApplicationUser { Id = userId, Email = "test@authentik.gov" };

            _userManagerMock.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.FindByLoginAsync(externalInfo.ProviderName, externalInfo.ProviderKey))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(u => u.AddLoginAsync(user, It.Is<UserLoginInfo>(li => li.LoginProvider == externalInfo.ProviderName && li.ProviderKey == externalInfo.ProviderKey)))
                .ReturnsAsync(IdentityResult.Success);

            await _federatedAuthService.LinkAccountAsync(userId, externalInfo);

            _userManagerMock.Verify(u => u.AddLoginAsync(user, It.Is<UserLoginInfo>(li => li.LoginProvider == externalInfo.ProviderName && li.ProviderKey == externalInfo.ProviderKey)), Times.Once);
        }

        /// <summary>
        /// Asserts that <see cref="FederatedAuthService.HandleCallbackAsync"/> successfully synchronizes roles and geographic profile mappings on callback.
        /// </summary>
        [Fact]
        public async Task HandleCallbackAsync_ShouldDeltaSyncRolesAndGeography_SuccessfulLogin()
        {
            var providerName = "Authentik";
            var userEmail = "test@authentik.gov";
            var userClaims = new Dictionary<string, string>
            {
                { "email", userEmail },
                { "given_name", "Test" },
                { "family_name", "User" },
                { "agency", "Utah DOT" },
                { "atspm_areas", "[1,2,5]" },
                { "atspm_regions", "[3]" },
                { "atspm_jurisdictions", "[4,7]" },
                { "roles", "UsageAdmin,DataAdmin" }
            };

            var externalInfo = new ExternalIdentityDto(providerName, "authentik-external-key-123", userClaims);
            var user = new ApplicationUser
            {
                Id = "user-id-999",
                Email = userEmail,
                FirstName = "Test",
                LastName = "User",
                Agency = "Old Agency"
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(userEmail))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.FindByLoginAsync(providerName, externalInfo.ProviderKey))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _geographyRepositoryMock.Setup(g => g.UpdateUserGeographyAsync(
                user.Id,
                It.Is<IEnumerable<int>>(areas => areas.SequenceEqual(new[] { 1, 2, 5 })),
                It.Is<IEnumerable<int>>(regions => regions.SequenceEqual(new[] { 3 })),
                It.Is<IEnumerable<int>>(jurisdictions => jurisdictions.SequenceEqual(new[] { 4, 7 }))
            )).Returns(Task.CompletedTask);

            _userManagerMock.Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "UsageAdmin", "Viewer" });

            _userManagerMock.Setup(u => u.AddToRolesAsync(user, It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { "DataAdmin" }))))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(u => u.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { "Viewer" }))))
                .ReturnsAsync(IdentityResult.Success);

            _signInManagerMock.Setup(s => s.SignInAsync(user, false, null))
                .Returns(Task.CompletedTask);

            var result = await _federatedAuthService.HandleCallbackAsync(providerName, externalInfo);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Utah DOT", user.Agency);

            _geographyRepositoryMock.Verify(g => g.UpdateUserGeographyAsync(
                user.Id,
                It.Is<IEnumerable<int>>(areas => areas.SequenceEqual(new[] { 1, 2, 5 })),
                It.Is<IEnumerable<int>>(regions => regions.SequenceEqual(new[] { 3 })),
                It.Is<IEnumerable<int>>(jurisdictions => jurisdictions.SequenceEqual(new[] { 4, 7 }))
            ), Times.Once);

            _userManagerMock.Verify(u => u.AddToRolesAsync(user, It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { "DataAdmin" }))), Times.Once);
            _userManagerMock.Verify(u => u.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { "Viewer" }))), Times.Once);
            _signInManagerMock.Verify(s => s.SignInAsync(user, false, null), Times.Once);
        }
    }
}
