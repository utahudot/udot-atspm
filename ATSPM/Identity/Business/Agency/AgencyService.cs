using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Business.Agency
{
    public class AgencyService : IAgencyService
    {

        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<bool> AgencyExistsAsync(string agencyName)
        {
            return await _userManager.Users.AnyAsync(u => u.Agency == agencyName);
        }
    }
}