#region license
// Copyright 2024 Utah Departement of Transportation
// for Identity - Identity.Controllers/UsersController.cs
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
using ATSPM.Identity.Business.Users;
using Identity.Business.Users;
using Identity.Models.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    //[Authorize()]
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly UsersService usersService;

        public UsersController(UserManager<ApplicationUser> userManager, UsersService usersService)
        {
            this.userManager = userManager;
            this.usersService = usersService;
        }

        [HttpGet]
        [Authorize(Policy = "CanViewUsers")]
        public async Task<IActionResult> GetUsersAsync([FromServices] IServiceScopeFactory serviceScopeFactory)
        {
            var usersDto = new List<UserDTO>();
            var users = userManager.Users;

            foreach (var user in users)
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    var userDto = new UserDTO
                    {
                        UserId = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Agency = user.Agency,
                        Email = user.Email,
                        UserName = user.UserName,
                        Roles = await scopedUserManager.GetRolesAsync(user)
                    };
                    usersDto.Add(userDto);
                }
            }

            return Ok(usersDto);
        }



        [HttpDelete("{userId}")]
        [Authorize(Policy = "CanDeleteUsers")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await userManager.DeleteAsync(user);
            if (result != null && result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result?.Errors);
            }
        }

        [HttpPost("update")]
        [Authorize(Policy = "CanEditUsers")]
        public async Task<IActionResult> AssignRole(UserDTO model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await usersService.updateUserFields(model);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}