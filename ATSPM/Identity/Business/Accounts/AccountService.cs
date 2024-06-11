using Identity.Business.Tokens;
using Microsoft.AspNetCore.Identity;

namespace Identity.Business.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService tokenService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            TokenService tokenService,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.tokenService = tokenService;
            this._roleManager = roleManager;
        }

        public async Task<AccountResult> CreateUser(ApplicationUser user, string password)
        {
            var createUserResult = await _userManager.CreateAsync(user, password);

            if (createUserResult.Succeeded)
            {
                //await userManager.AddToRoleAsync(user, "User");
                if (user.Email == null)
                {
                    return new AccountResult(StatusCodes.Status400BadRequest, "", new List<string>(), "Email is required");
                }
                return await Login(user.Email, password);
            }

            return new AccountResult(StatusCodes.Status400BadRequest, "", new List<string>(),
                createUserResult.Errors.First().Description);
        }


        public async Task<AccountResult> Login(string email, string password, bool rememberMe = false)
        {
            var user = await _signInManager.UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AccountResult(StatusCodes.Status400BadRequest, "", new List<string>(), "User not found");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var token = await tokenService.GenerateJwtTokenAsync(user);
                var viewClaims = await GetViewClaimsForUser(user);
                return new AccountResult(StatusCodes.Status200OK, token, viewClaims, null);
            }

            return new AccountResult(StatusCodes.Status400BadRequest, "", new List<string>(), "Incorrect username or password");
        }

        private async Task<List<string>> GetViewClaimsForUser(ApplicationUser user)
        {
            var claims = new List<string>();
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                claims.Add("Admin");
            }
            else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim.Value);
                    }
                }
            }

            return claims;
        }
    }
}