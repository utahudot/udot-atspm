#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityTests - Utah.Udot.Atspm.IdentityTests.Controllers/UsersServiceTests.cs
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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Xunit;

namespace Utah.Udot.Atspm.IdentityTests.Controllers
{
    public class UsersServiceTests
    {
        [Fact]
        public async Task UpdateUserFields_ReplacesExistingAssignments()
        {
            var user = new ApplicationUser
            {
                Id = "user-1",
                FirstName = "Old",
                LastName = "Name",
                Agency = "Old Agency",
                Email = "old@example.com",
                UserName = "old.user"
            };

            var userManagerMock = BuildUserManagerMock();
            userManagerMock.Setup(um => um.FindByIdAsync(user.Id)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Viewer" });
            userManagerMock.Setup(um => um.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            using var configContext = BuildConfigContext(nameof(UpdateUserFields_ReplacesExistingAssignments));
            configContext.UserAreas.Add(new UserArea { UserId = user.Id, AreaId = 1 });
            configContext.UserRegions.Add(new UserRegion { UserId = user.Id, RegionId = 2 });
            configContext.UserJurisdictions.Add(new UserJurisdiction { UserId = user.Id, JurisdictionId = 3 });
            await configContext.SaveChangesAsync();

            var service = new UsersService(userManagerMock.Object, configContext);
            var model = new UserDTO
            {
                UserId = user.Id,
                FirstName = "New",
                LastName = "User",
                Agency = "UDOT",
                Email = "new@example.com",
                UserName = "new.user",
                Roles = new List<string> { "Admin" },
                AreaIds = new List<int> { 4, 5 },
                RegionIds = new List<int> { 6 },
                JurisdictionIds = new List<int>()
            };

            await service.updateUserFields(model);

            Assert.Equal("New", user.FirstName);
            Assert.Equal("User", user.LastName);
            Assert.Equal("UDOT", user.Agency);
            Assert.Equal("new@example.com", user.Email);
            Assert.Equal("new.user", user.UserName);

            Assert.Equal(new[] { 4, 5 }, configContext.UserAreas.Where(x => x.UserId == user.Id).Select(x => x.AreaId).OrderBy(x => x).ToArray());
            Assert.Equal(new[] { 6 }, configContext.UserRegions.Where(x => x.UserId == user.Id).Select(x => x.RegionId).ToArray());
            Assert.Empty(configContext.UserJurisdictions.Where(x => x.UserId == user.Id));
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
