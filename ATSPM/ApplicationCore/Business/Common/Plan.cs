#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/Plan.cs
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
using ATSPM.Application.Analysis.Plans;
using System;

namespace ATSPM.Application.Business.Common
{
    public class Plan
    {
        public Plan(string planNumber, DateTime startTime, DateTime endTime)
        {
            PlanNumber = planNumber;
            Start = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            End = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);
            PlanDescription = getPlanDescription();
        }

        private string getPlanDescription()
        {
            var planDescription = "Unknown";
            switch (PlanNumber)
            {
                case "254":
                    planDescription = "Free";
                    break;
                case "255":
                    planDescription = "Flash";
                    break;
                case "0":
                    planDescription = "Unknown";
                    break;
                default:
                    planDescription = "Plan " + PlanNumber;

                    break;
            }

            return planDescription;
        }

        public string PlanNumber { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public string PlanDescription { get; }
    }
}
