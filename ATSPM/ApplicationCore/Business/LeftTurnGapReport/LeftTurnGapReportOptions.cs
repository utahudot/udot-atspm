#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LeftTurnGapReport/LeftTurnGapReportOptions.cs
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
using System;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class LeftTurnGapReportOptions
    {
        public string LocationIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int[] ApproachIds { get; set; }
        public int[] DaysOfWeek { get; set; }
        public int? StartHour { get; set; }
        public int? StartMinute { get; set; }
        public int? EndHour { get; set; }
        public int? EndMinute { get; set; }
        public bool GetAMPMPeakPeriod { get; set; }
        public bool GetAMPMPeakHour { get; set; }
        public bool Get24HourPeriod { get; set; }
        public bool GetGapReport { get; set; }
        public double AcceptableGapPercentage { get; set; }
        public bool GetSplitFail { get; set; }
        public double AcceptableSplitFailPercentage { get; set; }
        public bool GetPedestrianCall { get; set; }
        public bool GetConflictingVolume { get; set; }
    }
}
