#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PedDelay/PedDelayResult.cs
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

namespace ATSPM.Application.Business.PedDelay
{
    /// <summary>
    /// Ped Delay chart
    /// </summary>
    public class PedDelayResult : ApproachResult
    {
        public PedDelayResult(
            string locationId,
            int approachId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            int pedPresses,
            double cyclesWithPedRequests,
            int timeBuffered,
            int uniquePedestrianDetections,
            double minDelay,
            double maxDelay,
            double averageDelay,
            List<PedDelayPlan> plans,
            List<DataPointForDouble> cycleLengths,
            List<DataPointForDouble> pedestrianDelay,
            List<DataPointForDouble> startOfWalk,
            List<DataPointForDouble> percentDelayByCycleLength) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            PedPresses = pedPresses;
            CyclesWithPedRequests = cyclesWithPedRequests;
            TimeBuffered = timeBuffered;
            UniquePedestrianDetections = uniquePedestrianDetections;
            MinDelay = minDelay;
            MaxDelay = maxDelay;
            AverageDelay = averageDelay;
            Plans = plans;
            CycleLengths = cycleLengths;
            PedestrianDelay = pedestrianDelay;
            StartOfWalk = startOfWalk;
            PercentDelayByCycleLength = percentDelayByCycleLength;
        }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public int PedPresses { get; internal set; }
        public double CyclesWithPedRequests { get; internal set; }
        public int TimeBuffered { get; internal set; }
        public int UniquePedestrianDetections { get; internal set; }
        public double MinDelay { get; internal set; }
        public double MaxDelay { get; internal set; }
        public double AverageDelay { get; internal set; }
        public List<PedDelayPlan> Plans { get; internal set; }
        public List<DataPointForDouble> CycleLengths { get; internal set; }
        public List<DataPointForDouble> PedestrianDelay { get; internal set; }
        public List<DataPointForDouble> StartOfWalk { get; internal set; }
        public List<DataPointForDouble> PercentDelayByCycleLength { get; internal set; }

    }
}