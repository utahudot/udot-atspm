using Identity.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Identity.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var profileViewModel = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
                // Include other profile properties as needed
            };

            return Ok(profileViewModel);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            if (model == null || model.LastName == null || model.FirstName == null || model.Agency == null || model.Email == null || !ModelState.IsValid)
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
            // Update other profile properties as needed

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
