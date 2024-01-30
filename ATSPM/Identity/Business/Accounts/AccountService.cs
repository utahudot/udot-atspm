using Identity.Business.Agency;
using Identity.Business.Tokens;
using Microsoft.AspNetCore.Identity;


namespace Identity.Business.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService tokenService;
        private readonly IAgencyService _agencyService;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            IAgencyService agencyService,
            SignInManager<ApplicationUser> signInManager,
            TokenService tokenService)
        {
            _userManager = userManager;
            _agencyService = agencyService;
            _signInManager = signInManager;
            this.tokenService = tokenService;
        }

        public async Task<AccountResult> CreateUser(ApplicationUser user, string password)
        {
            var createUserResult = await _userManager.CreateAsync(user, password);

            if (createUserResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return await Login(user.Email, password);
            }

            return new AccountResult("", StatusCodes.Status400BadRequest,
                createUserResult.Errors.First().Description);
        }


        public async Task<AccountResult> Login(string email, string password, bool rememberMe = false)
        {
            var user = await _signInManager.UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AccountResult("", StatusCodes.Status400BadRequest, "User not found");
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var token = await tokenService.GenerateJwtTokenAsync(user);
                return new AccountResult(user.UserName, StatusCodes.Status200OK, token);
            }

            return new AccountResult("", StatusCodes.Status400BadRequest, "Incorrect username or password");
        }

    }
}