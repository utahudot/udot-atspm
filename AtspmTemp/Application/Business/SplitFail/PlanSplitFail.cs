﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.SplitFail/PlanSplitFail.cs
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

namespace Utah.Udot.Atspm.Business.SplitFail
{
    public class PlanSplitFail : Plan
    {
        public PlanSplitFail(DateTime start, DateTime end, string planNumber, List<CycleSplitFail> cycles) : base(planNumber, start,
            end)
        {
            var cyclesForPlan = cycles.Where(c => c.StartTime >= start && c.StartTime < end).ToList();
            if (cyclesForPlan.Count > 0)
            {
                TotalCycles = cyclesForPlan.Count;
                FailsInPlan = cyclesForPlan.Count(c => c.IsSplitFail);
                PercentFails = FailsInPlan / TotalCycles * 100;
            }
        }

        public double TotalCycles { get; }
        public double FailsInPlan { get; }
        public double PercentFails { get; }
    }
}