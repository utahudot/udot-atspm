#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PedDelay/Approaches.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


namespace ATSPM.Application.Business.PedDelay
{
    public static class Approaches
    {
        public enum LocationHeadType
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
            [Description("Protected")]
            Protected,
            [Description("Permissive")]
            Permissive,
            [Description("Protected/Permissive")]
            ProtectedPermissive
        }
        public static LocationHeadType GetLocationHeadType(this Approach approach)
        {
            int protectedPhaseNumber = approach.ProtectedPhaseNumber;
            int? permissivePhaseNumber = approach.PermissivePhaseNumber;

            if (protectedPhaseNumber > 0 && permissivePhaseNumber == null)
            {
                return LocationHeadType.ProtectedOnly;

            }

            if (protectedPhaseNumber == 0 && permissivePhaseNumber > 0)
            {
                return LocationHeadType.PermissiveOnly;
            }

            if (protectedPhaseNumber == 1 && permissivePhaseNumber == 6 ||
                 protectedPhaseNumber == 3 && permissivePhaseNumber == 8 ||
                 protectedPhaseNumber == 5 && permissivePhaseNumber == 2 ||
                 protectedPhaseNumber == 7 && permissivePhaseNumber == 4)
            {
                return LocationHeadType.FiveHead;
            }

            return LocationHeadType.FYA;
        }

        public static PhaseType GetPhaseType(this Approach approach)
        {
            int protectedPhaseNumber = approach.ProtectedPhaseNumber;
            int? permissivePhaseNumber = approach.PermissivePhaseNumber;

            if (protectedPhaseNumber > 0 && permissivePhaseNumber == null)
            {
                return PhaseType.Protected;
            }

            if (protectedPhaseNumber == 0 && permissivePhaseNumber > 0)
            {
                return PhaseType.Permissive;
            }

            return PhaseType.ProtectedPermissive;
        }

    }
}
