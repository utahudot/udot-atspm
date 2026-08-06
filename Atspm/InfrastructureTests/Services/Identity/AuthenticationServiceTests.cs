#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.Infrastructure.Services.Identity.Tests/AuthenticationServiceTests.cs
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Services.Identity.Dto;
using Utah.Udot.Atspm.Infrastructure.Services.Identity;
using Utah.Udot.NetStandardToolkit.Services;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.Identity.Tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="AuthenticationService"/> class.
    /// </summary>
    public class AuthenticationServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IOptions<IdentityConfiguration>> _identityOptionsMock;
        private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
        private readonly AuthenticationService _authenticationService;

        public AuthenticationServiceTests()
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

            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _identityOptionsMock = new Mock<IOptions<IdentityConfiguration>>();
            _loggerMock = new Mock<ILogger<AuthenticationService>>();

            _identityOptionsMock.Setup(o => o.Value).Returns(new IdentityConfiguration
            {
                Website = "https://localhost",
                DefaultEmailAddress = "no-reply@test.com"
            });

            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-32-chars-long-12345");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("AtspmIssuer");
            _configurationMock.Setup(c => c["Jwt:ExpireDays"]).Returns("7");

            _authenticationService = new AuthenticationService(
                _signInManagerMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _emailServiceMock.Object,
                _configurationMock.Object,
                _identityOptionsMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
        {
            // Arrange
            var request = new LoginRequestDto("nonexistent@test.com", "Password123!", false);
            _userManagerMock.Setup(u => u.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authenticationService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnResponseDto_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = "user@test.com",
                FirstName = "Test",
                LastName = "User",
                UserName = "user@test.com"
            };

            var request = new LoginRequestDto("user@test.com", "Password123!", false);

            _userManagerMock.Setup(u => u.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock.Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var response = await _authenticationService.LoginAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(user.Id, response.UserId);
            Assert.Equal(user.Email, response.Email);
            Assert.NotEmpty(response.AccessToken);
            Assert.Contains("User", response.Roles);
        }

        [Fact]
        public async Task LogoutAsync_ShouldSignOutSuccessfully()
        {
            // Act
            await _authenticationService.LogoutAsync("user-123");

            // Assert
            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
        }
    }
}
