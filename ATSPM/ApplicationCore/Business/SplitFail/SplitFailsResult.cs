#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.SplitFail/SplitFailsResult.cs
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

namespace ATSPM.Application.Business.SplitFail
{
    public class SplitFailsResult : ApproachResult
    {
        public SplitFailsResult(
            string locationId,
            int approachId,
            int phaseNumber,
            DateTime start,
            DateTime end,
            int totalSplitFails,
            ICollection<PlanSplitFail> plans,
            ICollection<DataPointBase> failLines,
            ICollection<DataPointForDouble> gapOutGreenOccupancies,
            ICollection<DataPointForDouble> gapOutRedOccupancies,
            ICollection<DataPointForDouble> forceOffGreenOccupancies,
            ICollection<DataPointForDouble> forceOffRedOccupancies,
            ICollection<DataPointForDouble> averageGor,
            ICollection<DataPointForDouble> averageRor,
            ICollection<DataPointForDouble> percentFails) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            TotalSplitFails = totalSplitFails;
            Plans = plans;
            FailLines = failLines;
            GapOutGreenOccupancies = gapOutGreenOccupancies;
            GapOutRedOccupancies = gapOutRedOccupancies;
            ForceOffGreenOccupancies = forceOffGreenOccupancies;
            ForceOffRedOccupancies = forceOffRedOccupancies;
            AverageGor = averageGor;
            AverageRor = averageRor;
            PercentFails = percentFails;
        }
        public int PhaseNumber { get; set; }
        public int TotalSplitFails { get; set; }
        public ICollection<PlanSplitFail> Plans { get; set; }
        public ICollection<DataPointBase> FailLines { get; set; }
        public ICollection<DataPointForDouble> GapOutGreenOccupancies { get; set; }
        public ICollection<DataPointForDouble> GapOutRedOccupancies { get; set; }
        public ICollection<DataPointForDouble> ForceOffGreenOccupancies { get; set; }
        public ICollection<DataPointForDouble> ForceOffRedOccupancies { get; set; }
        public ICollection<DataPointForDouble> AverageGor { get; set; }
        public ICollection<DataPointForDouble> AverageRor { get; set; }
        public ICollection<DataPointForDouble> PercentFails { get; set; }
    }
}
