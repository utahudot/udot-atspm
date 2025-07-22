#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.DTO/DetectorDto.cs
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

using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.ATSPM.ConfigApi.DTO
{
    public class DetectorDto
    {
        public int? Id { get; set; }
        public string DectectorIdentifier { get; set; }
        public int DetectorChannel { get; set; }
        public int? DistanceFromStopBar { get; set; }
        public int? MinSpeedFilter { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateDisabled { get; set; }
        public int? LaneNumber { get; set; }
        public MovementTypes MovementType { get; set; }
        public LaneTypes LaneType { get; set; }
        public DetectionHardwareTypes DetectionHardware { get; set; }
        public int? DecisionPoint { get; set; }
        public int? MovementDelay { get; set; }
        public double LatencyCorrection { get; set; }
        public int? ApproachId { get; set; }
        public ICollection<DetectionTypeDto> DetectionTypes { get; set; }
    }
}
