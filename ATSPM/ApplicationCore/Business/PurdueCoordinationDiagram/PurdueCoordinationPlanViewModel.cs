#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PurdueCoordinationDiagram/PurdueCoordinationPlanViewModel.cs
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

namespace ATSPM.Application.Business.PurdueCoordinationDiagram
{
    public class PerdueCoordinationPlanViewModel : Plan
    {
        //public string PlanNumber { get; set; }

        public PerdueCoordinationPlanViewModel(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            double percentGreenTime,
            double percentArrivalOnGreen,
            double platoonRatio) : base(planNumber, startTime, endTime)
        {
            //PlanNumber = planNumber;
            //Start = startTime;
            PercentGreenTime = percentGreenTime;
            PercentArrivalOnGreen = percentArrivalOnGreen;
            PlatoonRatio = platoonRatio;
            //End = endTime;
        }

        //public DateTime Start { get; set; }
        //public DateTime End { get; set; }
        public double PercentGreenTime { get; set; }
        public double PercentArrivalOnGreen { get; set; }
        public double PlatoonRatio { get; set; }
    }
}