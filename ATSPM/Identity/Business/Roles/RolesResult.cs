using System.ComponentModel.DataAnnotations;

namespace Identity.Business.NewFolder
{
    public class RolesResult
    {
        public required string Role { get; set; }

        public required List<string> Claims { get; set; }
    }
}
