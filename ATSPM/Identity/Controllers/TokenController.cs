#region license
// Copyright 2024 Utah Departement of Transportation
// for Identity - Identity.Controllers/TokenController.cs
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
using Identity.Models.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;

        public TokenController(UserManager<ApplicationUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("verify/reset")]
        public async Task<IActionResult> VerifyResetToken(VerifyResetTokenViewModel model)
        {
            if (string.IsNullOrEmpty(model.Token))
            {
                return BadRequest(new { Message = "Token is required." });
            }

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                return Unauthorized(new { Message = "User not found." });
            }

            // Verify the reset token
            var result = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, UserManager<ApplicationUser>.ResetPasswordTokenPurpose, model.Token);

            if (result)
            {
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                return Ok(new { Token = token, Message = "OK" });
            }
            else
            {
                return Unauthorized(new { Message = "Unauthorized" });
            }
        }

        [HttpPost("verify/connect")]
        public async Task<IActionResult> VerifyConnectToken(VerifyConnectTokenViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                return BadRequest(new { Message = "Username is required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Username);
            var roles = await _userManager.GetRolesAsync(user);

            if (user != null && roles.Contains("Admin"))
            {
                return Ok("Token verification successful. User has required claims.");
            }
            else
            {
                return Unauthorized("User does not have the required claims.");
            }
        }
    }
}
