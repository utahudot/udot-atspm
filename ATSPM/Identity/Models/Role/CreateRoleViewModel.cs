using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Role
{
    public class CreateRoleViewModel
    {
        [Required]
        public string RoleName { get; set; }
    }

}