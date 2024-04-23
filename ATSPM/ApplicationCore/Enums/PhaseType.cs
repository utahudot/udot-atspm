using System.ComponentModel;

namespace ATSPM.Application.Enums
{
    public enum PhaseType
    {
        [Description("Protected Only")]
        ProtectedOnly,
        [Description("Permissive Only")]
        PermissiveOnly,
        [Description("Protected/Permissive")]
        ProtectedPermissive
    }
}
