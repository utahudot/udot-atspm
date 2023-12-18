using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Token
{
    public class VerifyTokenViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
