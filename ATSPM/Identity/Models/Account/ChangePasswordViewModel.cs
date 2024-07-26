using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Account
{
    public class ChangePasswordViewModel
    {

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        public string resetToken { get; set; }

        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }

}