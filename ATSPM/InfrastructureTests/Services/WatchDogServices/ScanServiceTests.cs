#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests/ScanServiceTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2
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
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Repositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Services;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests
{
    public class ScanServiceTests
    {
        [Fact]
        public async Task GetWatchdogEmailRecipientsAsync_MapsRolesClaimsAndAssignments()
        {
            var users = new List<ApplicationUser>
            {
                new() { Id = "1", Email = "admin@example.com", FirstName = "Admin", LastName = "User" },
                new() { Id = "2", Email = "custom@example.com", FirstName = "Custom", LastName = "User" },
                new() { Id = "3", Email = "ignored@example.com", FirstName = "Ignored", LastName = "User" },
                new() { Id = "4", Email = "nameless@example.com", FirstName = null, LastName = null }
            };

            var userRoles = new Dictionary<string, IList<string>>
            {
                ["1"] = new List<string> { AtspmAuthorization.Roles.Admin, AtspmAuthorization.Roles.WatchdogSubscriber },
                ["2"] = new List<string> { "CustomWatchdogRole" },
                ["3"] = new List<string> { AtspmAuthorization.Roles.ReportAdmin },
                ["4"] = new List<string> { AtspmAuthorization.Roles.WatchdogSubscriber }
            };

            var roleClaims = new Dictionary<string, IList<Claim>>(StringComparer.OrdinalIgnoreCase)
            {
                [AtspmAuthorization.Roles.Admin] = new List<Claim>
                {
                    new(AtspmAuthorization.RoleClaimType, AtspmAuthorization.Permissions.Admin)
                },
                [AtspmAuthorization.Roles.WatchdogSubscriber] = new List<Claim>
                {
                    new(AtspmAuthorization.RoleClaimType, AtspmAuthorization.Permissions.WatchdogView),
                    new(AtspmAuthorization.RoleClaimType, AtspmAuthorization.Permissions.ReportView)
                },
                ["CustomWatchdogRole"] = new List<Claim>
                {
                    new(AtspmAuthorization.RoleClaimType, AtspmAuthorization.Permissions.WatchdogView)
                },
                [AtspmAuthorization.Roles.ReportAdmin] = new List<Claim>
                {
                    new(AtspmAuthorization.RoleClaimType, AtspmAuthorization.Permissions.ReportView)
                }
            };

            var userManager = CreateUserManagerMock(users, userRoles);
            var roleManager = CreateRoleManagerMock(roleClaims);

            var service = new ScanService(
                Mock.Of<ILocationRepository>(),
                Mock.Of<IWatchDogEventLogRepository>(),
                Mock.Of<IRegionsRepository>(),
                Mock.Of<IJurisdictionRepository>(),
                Mock.Of<IAreaRepository>(),
                Mock.Of<IUserRegionRepository>(),
                Mock.Of<IUserJurisdictionRepository>(),
                Mock.Of<IUserAreaRepository>(),
                userManager.Object,
                roleManager.Object,
                null!,
                null!,
                null!,
                Mock.Of<IWatchdogEmailService>(),
                Mock.Of<ILogger<ScanService>>(),
                null!,
                Mock.Of<IWatchDogIgnoreEventService>());

            var recipients = await service.GetWatchdogEmailRecipientsAsync(
                new List<UserRegion>
                {
                    new() { UserId = "1", RegionId = 100 },
                    new() { UserId = "2", RegionId = 200 }
                },
                new List<UserJurisdiction>
                {
                    new() { UserId = "1", JurisdictionId = 10 },
                    new() { UserId = "2", JurisdictionId = 20 }
                },
                new List<UserArea>
                {
                    new() { UserId = "1", AreaId = 1 },
                    new() { UserId = "4", AreaId = 4 }
                });

            Assert.Equal(3, recipients.Count);

            var adminRecipient = Assert.Single(recipients, r => r.UserId == "1");
            Assert.Equal("admin@example.com", adminRecipient.Email);
            Assert.Equal("Admin User", adminRecipient.DisplayName);
            Assert.True(adminRecipient.IsAdmin);
            Assert.True(adminRecipient.IsWatchdogSubscriber);
            Assert.True(adminRecipient.CanReceiveAllLocationsEmail);
            Assert.Equal(new List<int> { 100 }, adminRecipient.RegionIds);
            Assert.Equal(new List<int> { 10 }, adminRecipient.JurisdictionIds);
            Assert.Equal(new List<int> { 1 }, adminRecipient.AreaIds);

            var customRecipient = Assert.Single(recipients, r => r.UserId == "2");
            Assert.False(customRecipient.IsAdmin);
            Assert.False(customRecipient.IsWatchdogSubscriber);
            Assert.False(customRecipient.CanReceiveAllLocationsEmail);
            Assert.Equal(new List<int> { 200 }, customRecipient.RegionIds);
            Assert.Equal(new List<int> { 20 }, customRecipient.JurisdictionIds);
            Assert.Empty(customRecipient.AreaIds);

            var namelessRecipient = Assert.Single(recipients, r => r.UserId == "4");
            Assert.Equal(string.Empty, namelessRecipient.DisplayName);
            Assert.True(namelessRecipient.IsWatchdogSubscriber);
            Assert.False(namelessRecipient.IsAdmin);
            Assert.Equal(new List<int> { 4 }, namelessRecipient.AreaIds);

            Assert.DoesNotContain(recipients, r => r.UserId == "3");
        }

        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock(
            List<ApplicationUser> users,
            IDictionary<string, IList<string>> userRoles)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            userManager.Setup(m => m.Users).Returns(users.AsQueryable());
            userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync((ApplicationUser user) => userRoles.TryGetValue(user.Id, out var roles) ? roles : new List<string>());

            return userManager;
        }

        private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock(
            IDictionary<string, IList<Claim>> roleClaims)
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object,
                Array.Empty<IRoleValidator<IdentityRole>>(),
                null!,
                null!,
                null!);

            roleManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string roleName) => roleClaims.ContainsKey(roleName) ? new IdentityRole(roleName) : null);
            roleManager.Setup(m => m.GetClaimsAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync((IdentityRole role) => roleClaims.TryGetValue(role.Name ?? string.Empty, out var claims) ? claims : new List<Claim>());

            return roleManager;
        }
    }
}
