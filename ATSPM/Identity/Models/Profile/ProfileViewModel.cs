using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Profile
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Agency { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // Include other profile properties as needed
    }
}