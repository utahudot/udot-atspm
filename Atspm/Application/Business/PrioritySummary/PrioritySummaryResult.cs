#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.PrioritySummary/PrioritySummaryResult.cs
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
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.PrioritySummary
{
    /// <summary>
    /// Arrival On Red chart
    /// </summary>
    public class PrioritySummaryResult : LocationResult
    {
        public PrioritySummaryResult(string chartName,
            string locationId,
            DateTime start,
            DateTime end,
            TimeSpan averageDuration,
            double numberCheckins,
            double numberCheckouts,
            double numberEarlyGreens,
            double numberExtendedGreens,
            PrioritySummaryUnassignedEventsDto unassigned,
            ICollection<PrioritySummaryCycleDto> cycles,
            ICollection<IndianaEvent> indianaEvents) : base(locationId, start, end)
        {
            AverageDuration = averageDuration;
            NumberCheckins = numberCheckins;
            NumberCheckouts = numberCheckouts;
            NumberEarlyGreens = numberEarlyGreens;
            NumberExtendedGreens = numberExtendedGreens;
            Unassigned = unassigned;
            Events = indianaEvents;
            Cycles = cycles;
        }
        public TimeSpan AverageDuration { get; set; }
        public double NumberCheckins { get; set; }
        public double NumberCheckouts { get; set; }
        public double NumberEarlyGreens { get; set; }
        public double NumberExtendedGreens { get; set; }
        public PrioritySummaryUnassignedEventsDto Unassigned { get; set; }
        public ICollection<IndianaEvent> Events { get; set; }

        public ICollection<PrioritySummaryCycleDto> Cycles { get; set; }

    }
}
