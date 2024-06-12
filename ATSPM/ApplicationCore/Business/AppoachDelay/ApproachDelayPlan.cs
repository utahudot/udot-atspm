#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.AppoachDelay/ApproachDelayPlan.cs
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

namespace ATSPM.Application.Business.AppoachDelay
{
    public class ApproachDelayPlan : Plan
    {
        public ApproachDelayPlan(
            double averageDelay,
            double totalDelay,
            DateTime start,
            DateTime end,
            string planNumber,
            string planDescription) : base(planNumber, start, end)
        {
            AverageDelay = averageDelay;
            TotalDelay = totalDelay;
            Start = start;
            End = end;
            PlanNumber = planNumber;
            PlanDescription = planDescription;
        }

        public double AverageDelay { get; }
        public double TotalDelay { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public string PlanDescription { get; }
    }
}
