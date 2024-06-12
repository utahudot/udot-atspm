#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/PlanSplitMonitorData.cs
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
using System.Collections.Generic;

namespace ATSPM.Application.Business.Common
{
    public class PlanSplitMonitorData : Plan
    {
        public PlanSplitMonitorData(DateTime start, DateTime end, string planNumber) : base(planNumber, start, end)
        {
            Splits = new SortedDictionary<int, int>();
        }

        public SortedDictionary<int, int> Splits { get; set; }
        public int CycleLength { get; set; }
        public int OffsetLength { get; set; }
        public int HighCycleCount { get; set; }
        public double PercentSkips { get; set; }
        public double PercentGapOuts { get; set; }
        public double PercentMaxOuts { get; set; }
        public double PercentForceOffs { get; set; }
        public double AverageSplit { get; set; }
        public double PercentileSplit { get; set; }
        public double MinTime { get; set; }
        public double ProgrammedSplit { get; set; }
        public double PercentileSplit85th { get; set; }
        public double PercentileSplit50th { get; set; }
    }
}