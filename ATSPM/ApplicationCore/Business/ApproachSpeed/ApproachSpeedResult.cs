#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachSpeed/ApproachSpeedResult.cs
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
using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.ApproachSpeed
{
    public class ApproachSpeedResult : ApproachResult
    {
        public ApproachSpeedResult(
            string locationId,
            int approachId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            string detectionType,
            int distanceFromStopBar,
            double postedSpeed,
            ICollection<SpeedPlan> plans,
            ICollection<DataPointForInt> averageSpeeds,
            ICollection<DataPointForInt> eightyFifthSpeeds,
            ICollection<DataPointForInt> fifteenthSpeeds) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            DetectionType = detectionType;
            DistanceFromStopBar = distanceFromStopBar;
            PostedSpeed = postedSpeed;
            Plans = plans;
            AverageSpeeds = averageSpeeds;
            EightyFifthSpeeds = eightyFifthSpeeds;
            FifteenthSpeeds = fifteenthSpeeds;
        }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public string DetectionType { get; set; }
        public int DistanceFromStopBar { get; set; }
        public double PostedSpeed { get; set; }
        public ICollection<SpeedPlan> Plans { get; set; }
        public ICollection<DataPointForInt> AverageSpeeds { get; set; }
        public ICollection<DataPointForInt> EightyFifthSpeeds { get; set; }
        public ICollection<DataPointForInt> FifteenthSpeeds { get; set; }
    }
}