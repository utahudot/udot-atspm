#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.Infrastructure.Services.Identity.Tests/ApiKeyServiceTests.cs
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
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Services.Identity;
using Utah.Udot.Atspm.Repositories.IdentityRepositories;
using Utah.Udot.Atspm.Services.Identity.Dto;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.Identity.Tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="ApiKeyService"/> class.
    /// </summary>
    public class ApiKeyServiceTests
    {
        private readonly Mock<IApiKeyRepository> _apiKeyRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<ApiKeyService>> _loggerMock;
        private readonly ApiKeyService _apiKeyService;

        public ApiKeyServiceTests()
        {
            _apiKeyRepositoryMock = new Mock<IApiKeyRepository>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _loggerMock = new Mock<ILogger<ApiKeyService>>();
            _apiKeyService = new ApiKeyService(
                _apiKeyRepositoryMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateKeyAsync_ShouldSaveNewKeyAndReturnDetails()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-123", Email = "user@test.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
                new Claim(ClaimTypes.Role, "Admin")
            }));

            var createDto = new CreateApiKeyDto("Test Key", DateTimeOffset.UtcNow.AddDays(30), new List<string> { "Read", "Write" }, null);

            _userManagerMock.Setup(u => u.GetUserAsync(claimsPrincipal))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.GetUserId(claimsPrincipal))
                .Returns("user-123");

            _apiKeyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ApiKey>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _apiKeyService.CreateKeyAsync(createDto, claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Key", result.Name);
            Assert.NotEmpty(result.PlainTextKey);
            _apiKeyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ApiKey>()), Times.Once);
        }

        [Fact]
        public async Task GetKeysForUserAsync_ShouldReturnActiveKeysForOwner()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123")
            }));

            var activeKeys = new List<ApiKey>
            {
                new ApiKey { Id = 1, Name = "Key 1", OwnerId = "user-123", IsRevoked = false }
            };

            _userManagerMock.Setup(u => u.GetUserId(claimsPrincipal))
                .Returns("user-123");

            _apiKeyRepositoryMock.Setup(r => r.GetActiveKeysByOwnerAsync("user-123"))
                .ReturnsAsync(activeKeys);

            // Act
            var result = await _apiKeyService.GetKeysForUserAsync(claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Key 1", result.First().Name);
        }

        [Fact]
        public async Task RevokeKeyAsync_ShouldDeactivateKey_WhenKeyExists()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123")
            }));

            var keyId = 1;
            var existingKey = new ApiKey
            {
                Id = keyId,
                Name = "Test Key",
                OwnerId = "user-123",
                IsRevoked = false
            };

            _userManagerMock.Setup(u => u.GetUserId(claimsPrincipal))
                .Returns("user-123");

            _apiKeyRepositoryMock.Setup(r => r.GetKeyWithOwnerAsync(keyId, "user-123"))
                .ReturnsAsync(existingKey);

            _apiKeyRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _apiKeyService.RevokeKeyAsync(keyId, claimsPrincipal);

            // Assert
            Assert.True(result);
            Assert.True(existingKey.IsRevoked);
            _apiKeyRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ApiKey>()), Times.Once);
        }
    }
}
