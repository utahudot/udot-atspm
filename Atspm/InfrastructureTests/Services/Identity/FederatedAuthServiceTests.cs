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
using System.Security.Claims;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Services.Identity.Dto;
using Utah.Udot.Atspm.Infrastructure.Services.Identity;
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
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<FederatedAuthService>> _loggerMock;
        private readonly FederatedAuthService _federatedAuthService;

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

            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<FederatedAuthService>>();

            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-32-chars-long-12345");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("AtspmIssuer");
            _configurationMock.Setup(c => c["Jwt:ExpireDays"]).Returns("7");

            _federatedAuthService = new FederatedAuthService(
                _signInManagerMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _configurationMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task PrepareChallengeAsync_ShouldReturnChallengeProperties()
        {
            // Arrange
            var providerName = "Google";
            var redirectUri = "/signin-google";
            var authProperties = new AuthenticationProperties();
            authProperties.Items["test-key"] = "test-value";

            _signInManagerMock.Setup(s => s.ConfigureExternalAuthenticationProperties(providerName, redirectUri, null))
                .Returns(authProperties);

            // Act
            var result = await _federatedAuthService.PrepareChallengeAsync(providerName, redirectUri);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(providerName, result.Scheme);
            Assert.Equal(redirectUri, result.RedirectUri);
            Assert.Contains("test-key", result.Properties.Keys);
            Assert.Equal("test-value", result.Properties["test-key"]);
        }

        [Fact]
        public async Task LinkAccountAsync_ShouldAssociateExternalSsoWithUser()
        {
            // Arrange
            var userId = "user-123";
            var externalInfo = new ExternalIdentityDto("Google", "google-key-abc", new Dictionary<string, string>());
            var user = new ApplicationUser { Id = userId, Email = "test@google.com" };

            _userManagerMock.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.FindByLoginAsync(externalInfo.ProviderName, externalInfo.ProviderKey))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(u => u.AddLoginAsync(user, It.Is<UserLoginInfo>(li => li.LoginProvider == externalInfo.ProviderName && li.ProviderKey == externalInfo.ProviderKey)))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _federatedAuthService.LinkAccountAsync(userId, externalInfo);

            // Assert
            _userManagerMock.Verify(u => u.AddLoginAsync(user, It.Is<UserLoginInfo>(li => li.LoginProvider == externalInfo.ProviderName && li.ProviderKey == externalInfo.ProviderKey)), Times.Once);
        }
    }
}
