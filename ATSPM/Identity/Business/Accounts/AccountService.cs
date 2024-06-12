#region license
// Copyright 2024 Utah Departement of Transportation
// for Identity - Identity.Business.Accounts/AccountService.cs
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
                //await _userManager.AddToRoleAsync(user, "User");
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
                return new AccountResult( StatusCodes.Status200OK, token, viewClaims, null);
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
            } else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var roleClaim in roleClaims)
                    {
                        if (roleClaim.Value.ToLower().Contains("view"))
                        {
                           claims.Add(roleClaim.Value);
                        }
                    }
                }
            }
            
            return claims;
        }
    }
}