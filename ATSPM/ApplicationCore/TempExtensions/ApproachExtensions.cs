#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.TempExtensions/ApproachExtensions.cs
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
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.TempExtensions
{
    public static class ApproachExtensions
    {
        public static List<Detector> GetDetectorsForMetricType(this Approach approach, int metricTypeID)
        {
            var detectorsForMetricType = new List<Detector>();
            if (approach.Detectors != null)
            {
                foreach (var d in approach.Detectors)
                {
                    if (d.SupportsMetricType(metricTypeID))
                    {
                        detectorsForMetricType.Add(d);
                    }
                }
            }
            return detectorsForMetricType;
        }

        public static List<Detector> GetAllDetectorsOfDetectionType(this Approach approach, DetectionTypes detectionType)
        {
            if (approach.Detectors != null)
            {
                List<Detector> result = new List<Detector>();
                foreach (var d in approach.Detectors)
                    if (d.DetectionTypes != null)
                    {
                        var detectionTypes = d.DetectionTypes.Select(t => t.Id).ToList();
                        if (detectionTypes.Contains(detectionType))
                        {
                            result.Add(d);
                        }
                    }
                return result;
            }
            return new List<Detector>();
        }

        public static List<int> GetCycleEventCodes(this Approach approach, bool getPermissivePhase)
        {
            return getPermissivePhase && approach.IsPermissivePhaseOverlap || !getPermissivePhase && approach.IsProtectedPhaseOverlap
                ? new List<int> { 61, 63, 64, 66 }
                : new List<int> { 1, 8, 9 };
        }

        public static List<int> GetDetailedCycleEventCodes(this Approach approach, bool getPermissivePhase)
        {
            return getPermissivePhase && approach.IsPermissivePhaseOverlap || !getPermissivePhase && approach.IsProtectedPhaseOverlap
                ? new List<int> { 61, 63, 64, 66 }
                : new List<int> { 1, 3, 8, 9, 11 };
        }

        public static List<short> GetPedestrianCycleEventCodes(this Approach approach)
        {
            return approach.IsPedestrianPhaseOverlap ? new List<short>
            {
                67,
                68,
                45,
                90
            } : new List<short>
            {
                21,
                22,
                45,
                90
            };

        }

        public static List<int> GetPedDetectorsFromApproach(this Approach approach)
        {
            return !string.IsNullOrEmpty(approach.PedestrianDetectors) ? approach.PedestrianDetectors.Split(new char[] { ',', '-' }).Select(int.Parse).ToList() : new List<int>() { approach.ProtectedPhaseNumber };
        }

        public static PhaseType GetPhaseType(this Approach approach)
        {
            int protectedPhaseNumber = approach.ProtectedPhaseNumber;
            Nullable<int> permissivePhaseNumber = approach.PermissivePhaseNumber;

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

        public static SignalHeadType GetSignalHeadType(this Approach approach)
        {
            int protectedPhaseNumber = approach.ProtectedPhaseNumber;
            Nullable<int> permissivePhaseNumber = approach.PermissivePhaseNumber;

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


    }
}
