﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.GreenTimeUtilization/GreenTimeUtilizationResult.cs
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

namespace Utah.Udot.Atspm.Business.GreenTimeUtilization
{
    public class GreenTimeUtilizationResult : ApproachResult
    {
        public GreenTimeUtilizationResult()
        {
            Bins = new List<BarStack>();
            AverageSplits = new List<DataPointForDouble>();
            ProgrammedSplits = new List<DataPointForDouble>();
            Plans = new List<PlanSplitMonitorData>();
        }

        public GreenTimeUtilizationResult(
            int approachId,
            string locationIdentifier,
            DateTime start,
            DateTime end,
            List<BarStack> stacks,
            List<DataPointForDouble> avgSplits,
            List<DataPointForDouble> progSplits,
            int phaseNumber,
            int YAxisBinSize,
            int XAxisBinSize, List<PlanSplitMonitorData> plans) : base(approachId, locationIdentifier, start, end)
        {
            Bins = stacks;
            AverageSplits = avgSplits;
            ProgrammedSplits = progSplits;
            PhaseNumber = phaseNumber;
            this.YAxisBinSize = YAxisBinSize;
            this.XAxisBinSize = XAxisBinSize;
            Plans = plans;
        }

        public List<BarStack> Bins { get; set; }
        public List<DataPointForDouble> AverageSplits { get; set; }
        public List<DataPointForDouble> ProgrammedSplits { get; set; }
        public int PhaseNumber { get; set; }
        public int YAxisBinSize { get; set; }
        public int XAxisBinSize { get; set; }
        public List<PlanSplitMonitorData> Plans { get; }
    }
}