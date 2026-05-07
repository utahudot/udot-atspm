#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.PrioritySummary/PrioritySummaryService.cs
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public class PrioritySummaryService
    {
        //private readonly PlanService planService;

        // NOTE: matches your JS “recentlyClosed” behavior
        private static readonly TimeSpan Trailing119AttachWindow = TimeSpan.FromMilliseconds(1000);

        public PrioritySummaryService() // PlanService planService)
        {
            //this.planService = planService;
        }

        public PrioritySummaryResult GetChartData(
            PrioritySummaryOptions options,
            //IReadOnlyList<IndianaEvent> planEvents,
            IReadOnlyList<IndianaEvent> events)
        {
            var checkInEvents = events.Count(row => row.EventCode == 112);
            var checkOutEvents = events.Count(row => row.EventCode == 115);
            var earlyGreensEvents = events.Count(row => row.EventCode == 113);
            var extendedGreenEvents = events.Count(row => row.EventCode == 114);

            // Build cycles (backend equivalent of rollIntoCycles + finalizeCycle)
            var cycles = BuildCycles(options.LocationIdentifier, events, options.End);
            var unassigned = BuildUnassignedEvents(events, cycles);

            // Average duration: keep same semantics as your old code:
            // only include cycles that actually closed via a 115 (not report-end fallback)
            var durations = cycles
                .Where(c => c.RequestEndOffsetSec.HasValue) // has a checkout
                .Select(c => TimeSpan.FromSeconds(c.RequestEndOffsetSec!.Value))
                .ToList();

            var averageDuration = durations.Any()
                ? TimeSpan.FromTicks((long)durations.Average(d => d.Ticks))
                : TimeSpan.Zero;

            return new PrioritySummaryResult(
                "Priority Summary Service",
                options.LocationIdentifier,
                options.Start,
                options.End,
                averageDuration,
                checkInEvents,
                checkOutEvents,
                earlyGreensEvents,
                extendedGreenEvents,
                unassigned,
                cycles.ToList(),
                events.ToList()
            );
        }

        /* ---------------------------------------------
         * Cycle rolling (mirrors WebUI rollIntoCycles)
         * --------------------------------------------- */

        private sealed class MutableCycle
        {
            public string LocationIdentifier { get; init; } = "";
            public int TspNumber { get; init; }

            public DateTime CheckIn { get; set; }   // 112
            public DateTime CheckOut { get; set; }  // 115 or fallback

            public DateTime? ServiceStart { get; set; } // 118
            public DateTime? ServiceEnd { get; set; }   // 119

            public List<DateTime> Code113 { get; } = new();
            public List<DateTime> Code114 { get; } = new();
            public List<DateTime> Code116 { get; } = new();
            public List<DateTime> Code117 { get; } = new();

            // tracks whether we closed via a real 115 (vs report-end/forced)
            public bool ClosedBy115 { get; set; }
        }

        private sealed class RecentlyClosed
        {
            public required MutableCycle Cycle { get; init; }
            public required DateTime ClosedAt { get; init; }
        }

        private static IReadOnlyList<PrioritySummaryCycleDto> BuildCycles(
            string locationIdentifier,
            IReadOnlyList<IndianaEvent> events,
            DateTime reportEnd)
        {
            // JS sorts by timestamp, then uses a tie-break priority by eventCode
            var sorted = events
                .OrderBy(e => e.Timestamp)
                .ThenBy(e => CodePriority(e.EventCode))
                .ToList();

            // keyed by tspNumber (eventParam); options are per-location so locationIdentifier is constant
            var open = new Dictionary<int, MutableCycle>();
            var recentlyClosed = new Dictionary<int, RecentlyClosed>();
            var closed = new List<MutableCycle>();

            void PruneRecentlyClosed(DateTime now)
            {
                // prune items older than 1s relative to the current event time
                var toRemove = recentlyClosed
                    .Where(kvp => (now - kvp.Value.ClosedAt) > Trailing119AttachWindow)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var k in toRemove)
                    recentlyClosed.Remove(k);
            }

            foreach (var e in sorted)
            {
                PruneRecentlyClosed(e.Timestamp);

                var tsp = e.EventParam;
                open.TryGetValue(tsp, out var curr);

                if (e.EventCode == 112)
                {
                    // Force-close only the cycle for THIS tsp#
                    if (curr != null)
                    {
                        curr.CheckOut = e.Timestamp; // mirrors JS "force-close at next 112"
                        curr.ClosedBy115 = false;
                        closed.Add(curr);
                        open.Remove(tsp);
                    }

                    open[tsp] = new MutableCycle
                    {
                        LocationIdentifier = locationIdentifier,
                        TspNumber = tsp,
                        CheckIn = e.Timestamp,
                        CheckOut = e.Timestamp
                    };

                    continue;
                }

                // If no open cycle for this tsp#, only allow trailing 119 attach
                if (curr == null)
                {
                    if (e.EventCode == 119 && recentlyClosed.TryGetValue(tsp, out var recent))
                    {
                        var delta = e.Timestamp - recent.ClosedAt;
                        if (delta <= Trailing119AttachWindow)
                        {
                            // attach only if empty (same as JS)
                            if (!recent.Cycle.ServiceEnd.HasValue)
                                recent.Cycle.ServiceEnd = e.Timestamp;

                            // refresh entry (same semantics)
                            recentlyClosed[tsp] = new RecentlyClosed
                            {
                                Cycle = recent.Cycle,
                                ClosedAt = recent.ClosedAt
                            };
                        }
                    }

                    continue;
                }

                // Event attribution inside open cycle
                switch (e.EventCode)
                {
                    case 113: curr.Code113.Add(e.Timestamp); break;
                    case 114: curr.Code114.Add(e.Timestamp); break;
                    case 116: curr.Code116.Add(e.Timestamp); break;
                    case 117: curr.Code117.Add(e.Timestamp); break;
                    case 118:
                        if (!curr.ServiceStart.HasValue) curr.ServiceStart = e.Timestamp;
                        break;
                    case 119:
                        if (!curr.ServiceEnd.HasValue) curr.ServiceEnd = e.Timestamp;
                        break;
                }

                if (e.EventCode == 115)
                {
                    curr.CheckOut = e.Timestamp;
                    curr.ClosedBy115 = true;

                    closed.Add(curr);
                    recentlyClosed[tsp] = new RecentlyClosed { Cycle = curr, ClosedAt = e.Timestamp };
                    open.Remove(tsp);
                }
            }

            // Close any remaining open cycles at report end (JS behavior)
            foreach (var curr in open.Values)
            {
                curr.CheckOut = reportEnd;
                curr.ClosedBy115 = false;
                closed.Add(curr);
            }

            // finalize into DTO with offsets (seconds since 112)
            return closed.Select(FinalizeCycle).ToList();
        }

        private static PrioritySummaryUnassignedEventsDto BuildUnassignedEvents(
            IReadOnlyList<IndianaEvent> events,
            IReadOnlyList<PrioritySummaryCycleDto> cycles)
        {
            var cyclesByTsp = cycles
                .GroupBy(c => c.TspNumber)
                .ToDictionary(g => g.Key, g => g.ToList());

            var earlyGreen = new List<DataPointForInt>();
            var extendGreen = new List<DataPointForInt>();

            foreach (var e in events
                .Where(e => e.EventCode == 113 || e.EventCode == 114)
                .OrderBy(e => e.Timestamp)
                .ThenBy(e => e.EventCode))
            {
                if (cyclesByTsp.TryGetValue(e.EventParam, out var tspCycles) &&
                    tspCycles.Any(c => e.Timestamp >= c.CheckIn && e.Timestamp <= c.CheckOut))
                {
                    continue;
                }

                if (e.EventCode == 113)
                {
                    earlyGreen.Add(new DataPointForInt(e.Timestamp, e.EventParam));
                }
                else
                {
                    extendGreen.Add(new DataPointForInt(e.Timestamp, e.EventParam));
                }
            }

            return new PrioritySummaryUnassignedEventsDto(earlyGreen, extendGreen);
        }

        private static PrioritySummaryCycleDto FinalizeCycle(MutableCycle c)
        {
            double? requestEndOffsetSec = null;
            if (c.ClosedBy115 && c.CheckOut >= c.CheckIn)
                requestEndOffsetSec = (c.CheckOut - c.CheckIn).TotalSeconds;

            double? serviceStartOffsetSec = null;
            if (c.ServiceStart.HasValue && c.ServiceStart.Value >= c.CheckIn)
                serviceStartOffsetSec = (c.ServiceStart.Value - c.CheckIn).TotalSeconds;

            double? serviceEndOffsetSec = null;
            if (c.ServiceEnd.HasValue && c.ServiceEnd.Value >= c.CheckIn)
                serviceEndOffsetSec = (c.ServiceEnd.Value - c.CheckIn).TotalSeconds;

            return new PrioritySummaryCycleDto(
                c.LocationIdentifier,
                c.TspNumber,
                c.CheckIn,
                c.CheckOut,
                c.ServiceStart,
                c.ServiceEnd,
                c.Code113,
                c.Code114,
                c.Code116,
                c.Code117,
                requestEndOffsetSec,
                serviceStartOffsetSec,
                serviceEndOffsetSec
            );
        }

        private static int CodePriority(int code)
        {
            // Mirrors your JS "prio" function
            return code switch
            {
                112 => 0,
                113 or 114 or 116 or 117 or 118 => 1,
                115 => 2,
                119 => 3,
                _ => 4
            };
        }
    }
}
