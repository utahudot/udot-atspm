#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachSpeed/Plans.cs
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

namespace ATSPM.Application.Business.ApproachSpeed
{
    public class SpeedPlan : Plan
    {
        public SpeedPlan(
            DateTime start,
            DateTime end,
            string planNumber,
            int? averageSpeed,
            int? standardDeviation,
            int? eightyFifthPercentile,
            int? fifteenthPercentile) : base(planNumber, start, end)
        {
            AverageSpeed = averageSpeed;
            StandardDeviation = standardDeviation;
            EightyFifthPercentile = eightyFifthPercentile;
            FifteenthPercentile = fifteenthPercentile;
            //PlanNumber = planNumber;
            //Start = start;
            //End = end;
        }

        public int? AverageSpeed { get; set; }
        public int? StandardDeviation { get; set; }
        public int? EightyFifthPercentile { get; set; }
        public int? FifteenthPercentile { get; set; }
        //public string PlanNumber { get; set; }
        //public DateTime Start { get; set; }
        //public DateTime End { get; set; }

    }
}