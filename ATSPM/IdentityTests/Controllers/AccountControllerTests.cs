using Identity.Controllers;
using Identity.Models.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace YourProject.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly AccountController _accountController;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;

        public AccountControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);

            _accountController = new AccountController(_userManagerMock.Object, _signInManagerMock.Object);
        }

        [Fact]
        public async Task Register_ValidModel_ReturnsOk()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountController.Register(model);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterViewModel(); // Invalid model without required properties

            // Act
            var result = await _accountController.Register(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsOk()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                RememberMe = false
            };

            _signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _accountController.Login(model);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new LoginViewModel(); // Invalid model without required properties

            // Act
            var result = await _accountController.Login(model);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Logout_ReturnsOk()
        {
            // Act
            var result = await _accountController.Logout();

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ValidModel_ReturnsOk()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            var user = new ApplicationUser();

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountController.ChangePassword(model);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ChangePassword_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new ChangePasswordViewModel(); // Invalid model without required properties

            // Act
            var result = await _accountController.ChangePassword(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_ValidModel_ReturnsOk()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Email = "test@example.com"
            };

            var user = new ApplicationUser();

            _userManagerMock.Setup(um => um.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.IsEmailConfirmedAsync(user))
                .ReturnsAsync(true);

            _userManagerMock.Setup(um => um.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            // Act
            var result = await _accountController.ForgotPassword(model);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Email = null
            }; // Invalid model without required properties

            // Act
            var result = await _accountController.ForgotPassword(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
