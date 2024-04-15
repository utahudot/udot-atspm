using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Account
{
    public class VerifyUserPasswordResetViewModel
    {
        [Required]
        public string Password { get; set; }

    }
}
