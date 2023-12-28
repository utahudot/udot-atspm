using ATSPM.Domain.Extensions;
using Identity.Business.NewFolder;
using Identity.Models.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Controllers
{
    //[Authorize()]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        //[Authorize(Policy = "ViewRoles")]
        public async Task<IActionResult> GetRolesAsync()
        {
            var roles = _roleManager.Roles.ToList();
            var result = new List<RolesResult>();

            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                result.Add(new RolesResult { Role = role.Name, Claims = claims.Select(c => c.Value).ToList() });
            }

            return Ok(result);
        }

        [HttpPost]
        //[Authorize(Policy = "CreateRoles")]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = new IdentityRole { Name = model.RoleName };

            var result = await _roleManager.CreateAsync(role);
            if (result != null && result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result?.Errors);
            }
        }

        [HttpDelete("{roleName}")]
        [Authorize(Policy = "DeleteRoles")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result != null && result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result?.Errors);
            }
        }

        [HttpPost("assign")]
        [Authorize(Policy = "EditUsers")]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (model.UserId == null || model.RoleName == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
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