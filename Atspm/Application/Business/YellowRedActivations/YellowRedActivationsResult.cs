#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.YellowRedActivations/YellowRedActivationsResult.cs
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

using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Business.YellowRedActivations
{
    /// <summary>
    /// Yellow Red Activations chart
    /// </summary>
    public class YellowRedActivationsResult : ApproachResult
    {
        public YellowRedActivationsResult(
            string locationId,
            int approachId,
            string direction,
            string movementType,
            int protectedPhaseNumber,
            int? permissivePhaseNumber,
            bool isPermissivePhase,
            string phaseType,
            DateTime start,
            DateTime end,
            int totalViolations,
            int severeViolations,
            int yellowLightOccurences,
            ICollection<YellowRedActivationsPlan> plans,
            ICollection<DataPointForDouble> redEvents,
            ICollection<DataPointForDouble> yellowEvents,
            ICollection<DataPointForDouble> redClearanceEvents,
            ICollection<DataPointForDouble> detectorEvents) : base(approachId, locationId, start, end)
        {
            ApproachId = approachId;
            Direction = direction;
            MovementType = movementType;
            ProtectedPhaseNumber = protectedPhaseNumber;
            PermissivePhaseNumber = permissivePhaseNumber;
            IsPermissivePhase = isPermissivePhase;
            PhaseType = phaseType;
            TotalViolations = totalViolations;
            SevereViolations = severeViolations;
            YellowLightOccurences = yellowLightOccurences;
            Plans = plans;
            RedEvents = redEvents;
            YellowEvents = yellowEvents;
            RedClearanceEvents = redClearanceEvents;
            DetectorEvents = detectorEvents;
        }

        public string Direction { get; set; }
        public string MovementType { get; set; }
        public int ProtectedPhaseNumber { get; set; }
        public int? PermissivePhaseNumber { get; set; }
        public bool IsPermissivePhase { get; }
        public string PhaseType { get; set; }
        public int TotalViolations { get; set; }
        public int SevereViolations { get; set; }
        public int YellowLightOccurences { get; set; }
        public ICollection<YellowRedActivationsPlan> Plans { get; set; }
        public ICollection<DataPointForDouble> RedEvents { get; set; }
        public ICollection<DataPointForDouble> YellowEvents { get; set; }
        public ICollection<DataPointForDouble> RedClearanceEvents { get; set; }
        public ICollection<DataPointForDouble> DetectorEvents { get; set; }
    }
}