using Xunit;
using Identity.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Models.Profile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Identity.Controllers.Tests
{
    public class ProfileControllerTests
    {
        private readonly ProfileController _profileController;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public ProfileControllerTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null
            );

            _profileController = new ProfileController(_userManagerMock.Object);
        }

        [Fact]
        public async Task GetProfile_WithValidUser_ReturnsOk()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com", 
                Agency= "test", 
                // Set other properties as needed
            };

            _profileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email)
                        // Add other claims as needed
                    }))
                }
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Act
            var result = await _profileController.GetProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var profileViewModel = Assert.IsType<ProfileViewModel>(okResult.Value);

            Assert.Equal(user.FullName, $"{profileViewModel.FirstName} {profileViewModel.LastName}");
            Assert.Equal(user.Email, profileViewModel.Email);
            // Assert other profile properties as needed
        }

        [Fact]
        public async Task GetProfile_WithInvalidUser_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _profileController.GetProfile();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateProfile_WithValidModel_ReturnsNoContent()
        {
            // Arrange
            var model = new UpdateProfileViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Agency = "test"
                // Set other properties as needed
            };

            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "oldemail@example.com",
                Agency = "test"
                // Set other properties as needed
            };

            _profileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email)
                        // Add other claims as needed
                    }))
                }
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _profileController.UpdateProfile(model); // Use UpdateProfileViewModel

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal($"{model.FirstName} {model.LastName}", user.FullName);
            Assert.Equal(model.Email, user.Email);
            // Assert other updated profile properties as needed
        }



        [Fact] 
        public async Task UpdateProfile_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new UpdateProfileViewModel(); // Invalid model without required properties

            // Act
            var result = await _profileController.UpdateProfile(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}