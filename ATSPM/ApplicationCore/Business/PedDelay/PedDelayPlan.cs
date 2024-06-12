#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PedDelay/PedDelayPlan.cs
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

namespace ATSPM.Application.Business.PedDelay
{
    public class PedDelayPlan : Plan
    {
        public PedDelayPlan(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            string pedRecallMessage,
            int cyclesWithPedRequests,
            int uniquePedDetections,
            int pedPresses,
            double averageDelaySeconds,
            double averageCycleLengthSeconds) : base(planNumber, startTime, endTime)
        {
            PedRecallMessage = pedRecallMessage;
            CyclesWithPedRequests = cyclesWithPedRequests;
            UniquePedDetections = uniquePedDetections;
            PedPresses = pedPresses;
            AverageDelaySeconds = averageDelaySeconds;
            AverageCycleLengthSeconds = averageCycleLengthSeconds;
        }

        public string PedRecallMessage { get; internal set; }
        public int CyclesWithPedRequests { get; internal set; }
        public int UniquePedDetections { get; internal set; }
        public int PedPresses { get; internal set; }
        public double AverageDelaySeconds { get; internal set; }
        public double AverageCycleLengthSeconds { get; internal set; }

    }
}