using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


namespace ATSPM.ReportApi.Business.PedDelay
{
    public static class Approaches
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
        public enum PhaseType
        {
            [Description("Protected Only")]
            ProtectedOnly,
            [Description("Permissive Only")]
            PermissiveOnly,
            [Description("Protected/Permissive")]
            ProtectedPermissive
        }
        public static SignalHeadType GetSignalHeadType(this Approach approach)
        {
            int protectedPhaseNumber = approach.ProtectedPhaseNumber;
            int? permissivePhaseNumber = approach.PermissivePhaseNumber;

            if (protectedPhaseNumber > 0 && permissivePhaseNumber == null)
            {
                return SignalHeadType.ProtectedOnly;

            }

            if (protectedPhaseNumber == 0 && permissivePhaseNumber > 0)
            {
                return SignalHeadType.PermissiveOnly;
            }

            if (protectedPhaseNumber == 1 && permissivePhaseNumber == 6 ||
                 protectedPhaseNumber == 3 && permissivePhaseNumber == 8 ||
                 protectedPhaseNumber == 5 && permissivePhaseNumber == 2 ||
                 protectedPhaseNumber == 7 && permissivePhaseNumber == 4)
            {
                return SignalHeadType.FiveHead;
            }

            return SignalHeadType.FYA;
        }

        public static PhaseType GetPhaseType(this Approach approach)
        {
            int protectedPhaseNumber = approach.ProtectedPhaseNumber;
            int? permissivePhaseNumber = approach.PermissivePhaseNumber;

            if (protectedPhaseNumber > 0 && permissivePhaseNumber == null)
            {
                return PhaseType.ProtectedOnly;
            }

            if (protectedPhaseNumber == 0 && permissivePhaseNumber > 0)
            {
                return PhaseType.PermissiveOnly;
            }

            return PhaseType.ProtectedPermissive;
        }

    }
}
