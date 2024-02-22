using Identity.Business.Agency;
using Identity.Business.Tokens;
using Microsoft.AspNetCore.Identity;


namespace Identity.Business.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAgencyService _agencyService;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            IAgencyService agencyService,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _agencyService = agencyService;
            _signInManager = signInManager;
        }

        public async Task<AccountResult> CreateUser(ApplicationUser user, string password)
        {
            var tokenService = new TokenService("your_long_and_secure_key_here", "yourIssuer", "yourAudience");
            var agencyExists = await _agencyService.AgencyExistsAsync(user.Agency);
            var createUser = await _userManager.CreateAsync(user, password);

            // Just add new people as users
            //await _userManager.AddToRoleAsync(user, "User");

            if (createUser != null && createUser.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                var info = await _signInManager.UserManager.FindByEmailAsync(user.Email);
                var roles = await _signInManager.UserManager.GetRolesAsync(user);
                var token = tokenService.GenerateToken(user.Id, roles?.ToArray() ?? Array.Empty<string>(), user.Agency);

                if (info != null)
                {
                    return new AccountResult(user, roles?.ToList() ?? new List<string>(), token, StatusCodes.Status200OK, "");
                }
                else
                {
                    return new AccountResult(null, null, "", StatusCodes.Status500InternalServerError, "Server Error");
                }
            }

            return new AccountResult(null, null, "", StatusCodes.Status400BadRequest, createUser.Errors.First().Description);
        }

        public async Task<AccountResult> Login(string email, string password, bool rememberMe = false)
        {
            var tokenService = new TokenService("your_long_and_secure_key_here", "yourIssuer", "yourAudience");
            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);

            if (result != null && result.Succeeded)
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(email);
                var roles = await _signInManager.UserManager.GetRolesAsync(user);
                var token = tokenService.GenerateToken(user.Id, roles?.ToArray() ?? Array.Empty<string>(), user.Agency);

                return new AccountResult(user, roles?.ToList() ?? new List<string>(), token, StatusCodes.Status200OK, "");
            }

            return new AccountResult(null, null, "", StatusCodes.Status400BadRequest, "Incorrect username or password");
        }
    }
}