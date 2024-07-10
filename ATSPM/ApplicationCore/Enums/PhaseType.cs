using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Application.Enums
{
    public enum PhaseType
    {
        [Display(Name = "Protected Only")]
        ProtectedOnly,
        [Display(Name = "Permissive Only")]
        PermissiveOnly,
        [Display(Name = "Protected/Permissive")]
        ProtectedPermissive
    }
}
