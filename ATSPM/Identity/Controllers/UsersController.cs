﻿using Identity.Business.Users;
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

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
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

        [HttpPost("role/assign")]
        //[Authorize(Policy = "CanEditUsers")]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (model.UserId == null || model.RoleName == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await userManager.AddToRoleAsync(user, model.RoleName);
            if (result != null && result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result?.Errors);
            }
        }


    }
}