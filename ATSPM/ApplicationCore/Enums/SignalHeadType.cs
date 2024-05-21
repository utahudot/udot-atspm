using System.ComponentModel;

namespace ATSPM.Application.Enums
{
    public enum SignalHeadType
    {
        [Description("Protected Only")]
        ProtectedOnly,
        [Description("Permissive Only")]
        PermissiveOnly,
        [Description("5-Head")]
        FiveHead,
        [Description("Flashing Yellow Arrow")]
        FYA
    }
}
