#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.Infrastructure.Services.Identity.Tests/IdentityServiceTests.cs
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
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Services.Identity;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Services.Identity.Dto;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.Identity.Tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="IdentityService"/> class.
    /// </summary>
    public class IdentityServiceTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly IdentityContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IUserGeographyRepository> _geographyRepositoryMock;
        private readonly Mock<ILogger<IdentityService>> _loggerMock;
        private readonly IdentityService _identityService;

        public IdentityServiceTests()
        {
            _connection = new SqliteConnection("Datasource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<IdentityContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new IdentityContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _geographyRepositoryMock = new Mock<IUserGeographyRepository>();
            _loggerMock = new Mock<ILogger<IdentityService>>();

            _identityService = new IdentityService(
                _userManagerMock.Object,
                _context,
                _geographyRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldInvokeUserManagerAndSyncGeography()
        {
            // Arrange
            var createDto = new CreateUserRequestDto(
                "john@test.com", "John", "Doe", "AgencyA",
                new List<string> { "User" },
                new List<int> { 1, 2 },
                new List<int> { 3 },
                new List<int> { 4 }
            );

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, password) => user.Id = "new-user-id");

            _userManagerMock.Setup(u => u.AddToRolesAsync(It.IsAny<ApplicationUser>(), createDto.Roles))
                .ReturnsAsync(IdentityResult.Success);

            _geographyRepositoryMock.Setup(g => g.UpdateUserGeographyAsync(
                    "new-user-id", createDto.AreaIds, createDto.RegionIds, createDto.JurisdictionIds))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _identityService.CreateUserAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new-user-id", result.Id);
            Assert.Equal(createDto.Email, result.Email);
            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
            _geographyRepositoryMock.Verify(g => g.UpdateUserGeographyAsync(
                "new-user-id", createDto.AreaIds, createDto.RegionIds, createDto.JurisdictionIds), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUserDetailsWithRolesAndGeography()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser
            {
                Id = userId,
                Email = "john@test.com",
                FirstName = "John",
                LastName = "Doe",
                Agency = "AgencyA",
                UserName = "john@test.com"
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock.Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User", "Admin" });

            _geographyRepositoryMock.Setup(g => g.GetAreasByUserIdsAsync(It.Is<IEnumerable<string>>(ids => ids.Contains(userId))))
                .ReturnsAsync(new List<UserArea>
                {
                    new UserArea { UserId = userId, AreaId = 1 },
                    new UserArea { UserId = userId, AreaId = 2 }
                });

            _geographyRepositoryMock.Setup(g => g.GetRegionsByUserIdsAsync(It.Is<IEnumerable<string>>(ids => ids.Contains(userId))))
                .ReturnsAsync(new List<UserRegion>
                {
                    new UserRegion { UserId = userId, RegionId = 3 }
                });

            _geographyRepositoryMock.Setup(g => g.GetJurisdictionsByUserIdsAsync(It.Is<IEnumerable<string>>(ids => ids.Contains(userId))))
                .ReturnsAsync(new List<UserJurisdiction>
                {
                    new UserJurisdiction { UserId = userId, JurisdictionId = 4 }
                });

            // Act
            var result = await _identityService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Contains("User", result.Roles);
            Assert.Contains("Admin", result.Roles);
            Assert.Contains(1, result.AreaIds);
            Assert.Contains(3, result.RegionIds);
            Assert.Contains(4, result.JurisdictionIds);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
