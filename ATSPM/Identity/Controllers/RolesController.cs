using ATSPM.Identity.Business.Claims;
using Identity.Business.NewFolder;
using Identity.Models.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Identity.Controllers
{
    //[Authorize()]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ClaimsService claimsService;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ClaimsService claimsService)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.claimsService = claimsService;
        }

        [HttpGet]
        [Authorize(Policy = "CanViewRoles")]
        public async Task<IActionResult> GetRolesAsync()
        {
            var roles = roleManager.Roles.ToList();
            var result = new List<RolesResult>();

            foreach (var role in roles)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                result.Add(new RolesResult { Role = role.Name, Claims = claims.Select(c => c.Value).ToList() });
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "CanEditRoles")]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = new IdentityRole { Name = model.RoleName };

            var result = await roleManager.CreateAsync(role);
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
        [Authorize(Policy = "CanDeleteRoles")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound();
            }

            var result = await roleManager.DeleteAsync(role);
            if (result != null && result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result?.Errors);
            }
        }


        // POST: api/roles/setup
        [HttpPost("setup")]
        public async Task<IActionResult> Setup()
        {
            try
            {
                if (roleManager.Roles.AsNoTracking().Any())
                {
                    return BadRequest("Roles already exist.");
                }
                await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
                await roleManager.CreateAsync(new IdentityRole("UserAdmin"));
                await roleManager.CreateAsync(new IdentityRole { Name = "RoleAdmin" });
                await roleManager.CreateAsync(new IdentityRole { Name = "LocationConfigurationAdmin" });
                await roleManager.CreateAsync(new IdentityRole { Name = "GeneralConfigurationAdmin" });
                await roleManager.CreateAsync(new IdentityRole { Name = "DataAdmin" });
                await roleManager.CreateAsync(new IdentityRole { Name = "WatchdogSubscriber" });
                await roleManager.CreateAsync(new IdentityRole { Name = "ReportAdmin" });

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            //return BadRequest("Could not add claim to role.");
        }


        [HttpPost("setupClaims")]
        public async Task<IActionResult> SetupClaims()
        {
            var userAdminRole = roleManager.FindByNameAsync("UserAdmin").Result;
            await roleManager.AddClaimAsync(userAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:View"));
            await roleManager.AddClaimAsync(userAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:Edit"));
            await roleManager.AddClaimAsync(userAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User:Delete"));

            var adminRole = roleManager.FindByNameAsync("Admin").Result;
            await roleManager.AddClaimAsync(adminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin"));

            var roleAdminRole = await roleManager.FindByNameAsync("RoleAdmin");
            await roleManager.AddClaimAsync(roleAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:View"));
            await roleManager.AddClaimAsync(roleAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:Edit"));
            await roleManager.AddClaimAsync(roleAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Role:Delete"));

            var locationAdminRole = await roleManager.FindByNameAsync("LocationConfigurationAdmin");
            await roleManager.AddClaimAsync(locationAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:View"));
            await roleManager.AddClaimAsync(locationAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:Edit"));
            await roleManager.AddClaimAsync(locationAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "LocationConfiguration:Delete"));

            var generalAdminRole = await roleManager.FindByNameAsync("GeneralConfigurationAdmin");
            await roleManager.AddClaimAsync(generalAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:View"));
            await roleManager.AddClaimAsync(generalAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:Edit"));
            await roleManager.AddClaimAsync(generalAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "GeneralConfiguration:Delete"));

            var dataAdminRole = await roleManager.FindByNameAsync("DataAdmin");
            await roleManager.AddClaimAsync(dataAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Data:View"));
            await roleManager.AddClaimAsync(dataAdminRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Data:Edit"));

            var watchdogSubscriberRole = await roleManager.FindByNameAsync("WatchdogSubscriber");
            await roleManager.AddClaimAsync(watchdogSubscriberRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Watchdog:View"));
            await roleManager.AddClaimAsync(watchdogSubscriberRole, new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Report:View"));

            return Ok();
        }

    }
}