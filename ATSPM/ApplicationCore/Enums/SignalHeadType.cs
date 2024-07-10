using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Application.Enums
{
    public enum SignalHeadType
    {
        [Display(Name = "Protected Only")]
        ProtectedOnly,
        [Display(Name = "Permissive Only")]
        PermissiveOnly,
        [Display(Name = "5-Head")]
        FiveHead,
        [Display(Name = "Flashing Yellow Arrow")]
        FYA
    }
}
