using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Role
{
    public class AssignRoleViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string[] RoleNames { get; set; }
    }

}