using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Token
{
    public class VerifyResetTokenViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
