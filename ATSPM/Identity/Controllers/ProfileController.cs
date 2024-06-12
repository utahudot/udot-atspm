#region license
// Copyright 2024 Utah Departement of Transportation
// for Identity - Identity.Controllers/ProfileController.cs
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
using Identity.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var profileViewModel = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Agency = user.Agency,
                Roles = string.Join(",", roles)
            };

            return Ok(profileViewModel);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            if (model == null || model.LastName == null || model.FirstName == null || model.Email == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Agency = model.Agency;
            if(model.PhoneNumber != null)
            {
                user.PhoneNumber = model.PhoneNumber;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                // Handle update failure
                // You can return a BadRequest or provide an appropriate error response
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}