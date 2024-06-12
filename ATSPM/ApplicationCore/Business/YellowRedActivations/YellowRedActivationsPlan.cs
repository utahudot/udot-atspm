#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.YellowRedActivations/YellowRedActivationsPlan.cs
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

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivationsPlan : Plan
    {
        public YellowRedActivationsPlan(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            int totalViolations,
            int severeViolations,
            double percentViolations,
            double percentSevereViolations,
            double averageTimeViolations) : base(planNumber, startTime, endTime)
        {
            TotalViolations = totalViolations;
            SevereViolations = severeViolations;
            PercentViolations = percentViolations;
            PercentSevereViolations = percentSevereViolations;
            AverageTimeViolations = averageTimeViolations;
        }

        public int TotalViolations { get; internal set; }
        public int SevereViolations { get; internal set; }
        public double PercentViolations { get; internal set; }
        public double PercentSevereViolations { get; internal set; }
        public double AverageTimeViolations { get; internal set; }
    }
}