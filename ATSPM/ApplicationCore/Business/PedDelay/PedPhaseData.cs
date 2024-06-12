#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PedDelay/PedPhaseData.cs
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
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PedDelay
{
    public class PedPhaseData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Approach Approach { get; set; }
        public int ApproachId { get; set; }
        public int PhaseNumber { get; set; }
        public string locationId { get; set; }
        public List<PedCycle> Cycles { get; set; }
        public List<PedPlan> Plans { get; set; }
        public List<DataPointForDouble> HourlyTotals { get; set; }
        public double MinDelay { get; set; }
        public double AverageDelay { get; set; }
        public double MaxDelay { get; set; }
        public double TotalDelay { get; set; }
        public int TimeBuffer { get; set; }
        public int PedPresses { get; set; }
        public int UniquePedDetections { get; set; }
        public int PedRequests { get; set; }
        public int ImputedPedCallsRegistered { get; set; }
        public int PedBeginWalkCount { get; set; }
        public List<IndianaEvent> PedBeginWalkEvents { get; set; }
        public int PedCallsRegisteredCount { get; set; }
        public short BeginWalkEvent { get; set; }
        public short BeginClearanceEvent { get; set; }
    }

}
