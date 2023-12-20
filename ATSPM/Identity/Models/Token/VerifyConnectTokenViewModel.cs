using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Token
{
    public class VerifyConnectTokenViewModel
    {
        [Required]
        public string Username { get; set; }
    }
}
