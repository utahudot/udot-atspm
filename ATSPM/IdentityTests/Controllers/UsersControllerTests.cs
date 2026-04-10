#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityTests - Utah.Udot.Atspm.IdentityTests.Controllers/UsersControllerTests.cs
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

using Identity.Business.Users;
using Identity.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Xunit;

namespace Utah.Udot.Atspm.IdentityTests.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task GetUsersAsync_WithAssignments_ReturnsEnrichedUserDto()
        {
            var user = new ApplicationUser
            {
                Id = "user-1",
                FirstName = "Casey",
                LastName = "Jones",
                Agency = "UDOT",
                Email = "casey@example.com",
                UserName = "casey"
            };

            var userManagerMock = BuildUserManagerMock();
            userManagerMock.Setup(um => um.Users).Returns(new[] { user }.AsQueryable());
            userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

            using var configContext = BuildConfigContext(nameof(GetUsersAsync_WithAssignments_ReturnsEnrichedUserDto));
            configContext.Areas.Add(new Area { Id = 4, Name = "Central" });
            configContext.Regions.Add(new Region { Id = 7, Description = "North" });
            configContext.Jurisdictions.Add(new Jurisdiction { Id = 9, Name = "Salt Lake City" });
            configContext.UserAreas.Add(new UserArea { UserId = user.Id, AreaId = 4 });
            configContext.UserRegions.Add(new UserRegion { UserId = user.Id, RegionId = 7 });
            configContext.UserJurisdictions.Add(new UserJurisdiction { UserId = user.Id, JurisdictionId = 9 });
            await configContext.SaveChangesAsync();

            var usersService = new UsersService(userManagerMock.Object, configContext);
            var controller = new UsersController(userManagerMock.Object, usersService, configContext);

            var services = new ServiceCollection();
            services.AddSingleton(userManagerMock.Object);
            using var serviceProvider = services.BuildServiceProvider();

            var result = await controller.GetUsersAsync(serviceProvider.GetRequiredService<IServiceScopeFactory>());

            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsAssignableFrom<List<UserDTO>>(okResult.Value);
            var dto = Assert.Single(users);

            Assert.Equal(user.Id, dto.UserId);
            Assert.Equal(new[] { "Admin" }, dto.Roles);
            Assert.Equal(new[] { 4 }, dto.AreaIds);
            Assert.Equal(new[] { 7 }, dto.RegionIds);
            Assert.Equal(new[] { 9 }, dto.JurisdictionIds);
            Assert.Equal("Central", Assert.Single(dto.Areas).Name);
            Assert.Equal("North", Assert.Single(dto.Regions).Description);
            Assert.Equal("Salt Lake City", Assert.Single(dto.Jurisdictions).Name);
        }

        private static Mock<UserManager<ApplicationUser>> BuildUserManagerMock()
        {
            return new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
        }

        private static ConfigContext BuildConfigContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ConfigContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            return new ConfigContext(options);
        }
    }
}
