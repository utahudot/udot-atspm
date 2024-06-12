#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ArrivalOnRed/ArrivalOnRedResult.cs
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

namespace ATSPM.Application.Business.ArrivalOnRed
{
    /// <summary>
    /// Arrival On Red chart
    /// </summary>
    public class ArrivalOnRedResult : ApproachResult
    {
        public ArrivalOnRedResult(
            string locationId,
            int approachId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            double totalDetectorHits,
            double totalArrivalOnRed,
            double percentArrivalOnRed,
            ICollection<ArrivalOnRedPlan> plans,
            ICollection<DataPointForDouble> percentArrivalsOnRed,
            ICollection<DataPointForDouble> totalVehicles,
            ICollection<DataPointForDouble> arrivalsOnRed) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            TotalDetectorHits = totalDetectorHits;
            TotalArrivalOnRed = totalArrivalOnRed;
            PercentArrivalOnRed = percentArrivalOnRed;
            Plans = plans;
            PercentArrivalsOnRed = percentArrivalsOnRed;
            TotalVehicles = totalVehicles;
            ArrivalsOnRed = arrivalsOnRed;
        }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public double TotalDetectorHits { get; set; }
        public double TotalArrivalOnRed { get; set; }
        public double PercentArrivalOnRed { get; set; }
        public ICollection<ArrivalOnRedPlan> Plans { get; set; }
        public ICollection<DataPointForDouble> PercentArrivalsOnRed { get; set; }
        public ICollection<DataPointForDouble> TotalVehicles { get; set; }
        public ICollection<DataPointForDouble> ArrivalsOnRed { get; set; }

    }
}