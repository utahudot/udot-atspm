#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Identity.Controllers/UsersController.cs
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

using Asp.Versioning;
using Identity.Business.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.ATSPM.IdentityApi.Controllers;

namespace Identity.Controllers
{
    //[Authorize()]
    [ApiVersion("1.0")]
    public class UsersController : IdentityControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly UsersService usersService;
        private readonly ConfigContext configContext;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            UsersService usersService,
            ConfigContext configContext)
        {
            this.userManager = userManager;
            this.usersService = usersService;
            this.configContext = configContext;
        }

        [HttpGet]
        [AuthorizePermission(AtspmAuthorization.Permissions.UsersView)]
        public async Task<IActionResult> GetUsersAsync([FromServices] IServiceScopeFactory serviceScopeFactory)
        {
            var usersDto = new List<UserDTO>();
            var users = userManager.Users.OrderBy(u => u.UserName).ToList();
            var userIds = users.Select(u => u.Id).ToList();

            var userAreas = await configContext.UserAreas
                .Where(x => userIds.Contains(x.UserId))
                .Include(x => x.Area)
                .ToListAsync();
            var userRegions = await configContext.UserRegions
                .Where(x => userIds.Contains(x.UserId))
                .Include(x => x.Region)
                .ToListAsync();
            var userJurisdictions = await configContext.UserJurisdictions
                .Where(x => userIds.Contains(x.UserId))
                .Include(x => x.Jurisdiction)
                .ToListAsync();

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
                        Email = user.Email ?? string.Empty,
                        UserName = user.UserName ?? string.Empty,
                        Roles = await scopedUserManager.GetRolesAsync(user),
                        Areas = userAreas
                            .Where(x => x.UserId == user.Id)
                            .Select(x => new UserAreaDTO
                            {
                                Id = x.AreaId,
                                Name = x.Area?.Name ?? string.Empty
                            })
                            .OrderBy(x => x.Name)
                            .ToList(),
                        AreaIds = userAreas
                            .Where(x => x.UserId == user.Id)
                            .Select(x => x.AreaId)
                            .OrderBy(x => x)
                            .ToList(),
                        Regions = userRegions
                            .Where(x => x.UserId == user.Id)
                            .Select(x => new UserRegionDTO
                            {
                                Id = x.RegionId,
                                Description = x.Region?.Description ?? string.Empty
                            })
                            .OrderBy(x => x.Description)
                            .ToList(),
                        RegionIds = userRegions
                            .Where(x => x.UserId == user.Id)
                            .Select(x => x.RegionId)
                            .OrderBy(x => x)
                            .ToList(),
                        Jurisdictions = userJurisdictions
                            .Where(x => x.UserId == user.Id)
                            .Select(x => new UserJurisdictionDTO
                            {
                                Id = x.JurisdictionId,
                                Name = x.Jurisdiction?.Name ?? string.Empty
                            })
                            .OrderBy(x => x.Name)
                            .ToList(),
                        JurisdictionIds = userJurisdictions
                            .Where(x => x.UserId == user.Id)
                            .Select(x => x.JurisdictionId)
                            .OrderBy(x => x)
                            .ToList()
                    };
                    usersDto.Add(userDto);
                }
            }

            return Ok(usersDto);
        }



        [HttpDelete("{userId}")]
        [AuthorizePermission(AtspmAuthorization.Permissions.UsersDelete)]
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
        [AuthorizePermission(AtspmAuthorization.Permissions.UsersEdit)]
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
