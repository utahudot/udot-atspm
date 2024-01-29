using Identity.Business.Agency;
using Identity.Business.Tokens;
using Microsoft.AspNetCore.Identity;


namespace Identity.Business.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AccountResult> CreateUser(ApplicationUser user, string password)
        {
            var tokenService = new TokenService("your_long_and_secure_key_here");
            var createUser = await _userManager.CreateAsync(user, password);

            // Just add new people as users
            await _userManager.AddToRoleAsync(user, "User");

            if (createUser != null && createUser.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                var info = await _signInManager.UserManager.FindByEmailAsync(user.Email);

                if (info != null)
                {

                    var roles = await _signInManager.UserManager.GetRolesAsync(user);
                    var token = tokenService.GenerateToken(user.Id, roles?.ToArray() ?? Array.Empty<string>());
                    return new AccountResult(user.UserName, token, StatusCodes.Status200OK, "");
                }
                else
                {
                    return new AccountResult("", "", StatusCodes.Status500InternalServerError, "Server Error");
                }
            }

            return new AccountResult("", "", StatusCodes.Status400BadRequest, createUser.Errors.First().Description);
        }

        public async Task<AccountResult> Login(string email, string password, bool rememberMe = false)
        {
            var tokenService = new TokenService("your_long_and_secure_key_here");
            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);

            if (result != null && result.Succeeded)
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new AccountResult("", "", StatusCodes.Status400BadRequest, "User not found");
                }

                var roles = await _signInManager.UserManager.GetRolesAsync(user);
                var token = tokenService.GenerateToken(user.Id, roles?.ToArray() ?? Array.Empty<string>());

                return new AccountResult(user.UserName, token, StatusCodes.Status200OK, "");
            }

            return new AccountResult("", "", StatusCodes.Status400BadRequest, "Incorrect username or password");
        }
    }
}