using Identity.Models.Role;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Identity.Controllers.Tests
{
    
        public class RolesControllerTests
        {
            private readonly RolesController _rolesController;
            private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
            private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

            public RolesControllerTests()
            {
                _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                    Mock.Of<IRoleStore<IdentityRole>>(),
                    null, null, null, null
                );

                _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                    Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null
                );

                _rolesController = new RolesController(_roleManagerMock.Object, _userManagerMock.Object);
            }

            [Fact]
            public void GetRoles_ReturnsOkWithRoles()
            {
                // Arrange
                var roles = new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("User")
            };

                _roleManagerMock.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

                // Act
                var result = _rolesController.GetRoles();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var resultRoles = Assert.IsType<List<string>>(okResult.Value);

                Assert.Equal(roles.Count, resultRoles.Count);
                Assert.Equal(roles.Select(r => r.Name), resultRoles);
            }

            [Fact]
            public async Task CreateRole_WithValidModel_ReturnsOk()
            {
                // Arrange
                var model = new CreateRoleViewModel
                {
                    RoleName = "NewRole"
                };

                _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                    .ReturnsAsync(IdentityResult.Success);

                // Act
                var result = await _rolesController.CreateRole(model);

                // Assert
                Assert.IsType<OkResult>(result);
            }

            [Fact]
            public async Task CreateRole_WithInvalidModel_ReturnsBadRequest()
            {
                // Arrange
                var model = new CreateRoleViewModel(); // Invalid model without required properties

                // Act
                var result = await _rolesController.CreateRole(model);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task DeleteRole_WithExistingRole_ReturnsNoContent()
            {
                // Arrange
                var roleName = "RoleToDelete";

                var role = new IdentityRole(roleName);
                _roleManagerMock.Setup(rm => rm.FindByNameAsync(roleName))
                    .ReturnsAsync(role);

                _roleManagerMock.Setup(rm => rm.DeleteAsync(It.IsAny<IdentityRole>()))
                    .ReturnsAsync(IdentityResult.Success);

                // Act
                var result = await _rolesController.DeleteRole(roleName);

                // Assert
                Assert.IsType<NoContentResult>(result);
            }

            [Fact]
            public async Task DeleteRole_WithNonExistingRole_ReturnsNotFound()
            {
                // Arrange
                var roleName = "NonExistingRole";

                _roleManagerMock.Setup(rm => rm.FindByNameAsync(roleName))
                    .ReturnsAsync((IdentityRole)null);

                // Act
                var result = await _rolesController.DeleteRole(roleName);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task AssignRole_WithValidModel_ReturnsOk()
            {
                // Arrange
                var model = new AssignRoleViewModel
                {
                    UserId = "UserId",
                    RoleName = "RoleName"
                };

                var user = new ApplicationUser();
                _userManagerMock.Setup(um => um.FindByIdAsync(model.UserId))
                    .ReturnsAsync(user);

                _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);

                // Act
                var result = await _rolesController.AssignRole(model);

                // Assert
                Assert.IsType<OkResult>(result);
            }

            [Fact]
            public async Task AssignRole_WithInvalidModel_ReturnsBadRequest()
            {
                // Arrange
                var model = new AssignRoleViewModel(); // Invalid model without required properties

                // Act
                var result = await _rolesController.AssignRole(model);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }
    

}