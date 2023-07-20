using Identity.Models.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Identity.Controllers
{
    [Authorize(Roles = "Admin")] // Requires "Admin" role for accessing this controller
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
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }

        [HttpPost]
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
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (model.UserId == null || model.RoleName == null ||!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (result !=null && result.Succeeded)
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
