#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PedDelay/PedPlan.cs
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
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.PedDelay
{
    public class PedPlan
    {
        public PedPlan(int phaseNumber, DateTime startDate, DateTime endDate, int planNumber)
        {
            Start = startDate;
            End = endDate;
            PlanNumber = planNumber;
            PhaseNumber = phaseNumber;
        }

        public DateTime Start { get; }
        public DateTime End { get; }
        public int PlanNumber { get; }
        public int PhaseNumber { get; }
        public List<IndianaEvent> Events { get; set; }
        public List<PedCycle> Cycles { get; set; } = new List<PedCycle>();
        public int UniquePedDetections { get; set; }
        public int PedPresses { get; set; }
        public double CyclesWithPedRequests => Cycles.Count;
        public double PedBeginWalkCount
        {
            get
            {
                return Events.Where(e => e.EventCode == 21 || e.EventCode == 67).Count();
            }
        }
        public double PedCallsRegisteredCount
        {
            get
            {
                return Events.Where(e => e.EventCode == 45).Count();
            }
        }
        public double MinDelay
        {
            get
            {
                if (CyclesWithPedRequests > 0)
                    return Cycles.Min(c => c.Delay);
                return 0;
            }
        }

        public double MaxDelay
        {
            get
            {
                if (CyclesWithPedRequests > 0)
                    return Cycles.Max(c => c.Delay);
                return 0;
            }
        }

        public double AvgDelay
        {
            get
            {
                if (CyclesWithPedRequests > 0)
                    return Cycles.Average(c => c.Delay);
                return 0;
            }
        }
    }
}