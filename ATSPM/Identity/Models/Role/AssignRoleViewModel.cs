using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Role
{
    public class AssignRoleViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string RoleName { get; set; }
    }

}
