#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/IndianaEventExtensions.cs
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

using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Analysis.PedestrianDelay;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Provides extension methods for working with collections of <see cref="IndianaEvent"/> objects.
    /// </summary>
    public static class IndianaEventExtensions
    {
        public static IEnumerable<IndianaEvent> WhereCode(this IEnumerable<IndianaEvent> events, params short[] codes)
        {
            var codeSet = new HashSet<short>(codes);
            return events.Where(e => codeSet.Contains(e.EventCode));
        }

        public static IEnumerable<IndianaEvent> WhereParam(this IEnumerable<IndianaEvent> events, params short[] param)
        {
            var paramSet = new HashSet<short>(param);
            return events.Where(e => paramSet.Contains(e.EventParam));
        }

        public static ILookup<short, IndianaEvent> ToCodeLookup(this IEnumerable<IndianaEvent> events)
        {
            return events.ToLookup(e => e.EventCode);
        }

        public static ILookup<short, IndianaEvent> ToParamLookup(this IEnumerable<IndianaEvent> events)
        {
            return events.ToLookup(e => e.EventParam);
        }

        public static int CountImputedCalls(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            var filter = events
                .Where(e => e.EventCode is 90 or 21 or 67 or 0)
                .OrderBy(e => e.Timestamp)
                .ToList();

            return filter.Where((e, i) =>
                    i > 0 &&
                    range.InRange(e.Timestamp) &&
                    e.EventCode == 90 &&
                    filter[i - 1].EventCode is 21 or 67 or 0)
                .Count();
        }

        public static int CountUniquePedDetections(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            var filter = events
                .Where(w => w.EventCode == 90)
                .OrderBy(o => o.Timestamp)
                .ToList();

            return filter.SlidingWindow(2)
                .Where(x => x[1].Timestamp - x[0].Timestamp > TimeSpan.FromSeconds(15))
                .Count();
                //.Count(w => range.InRange(w[1].Timestamp));
        }

        public static int PedRequests(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            return events.Count(w => w.EventCode == (short)IndianaEnumerations.PedDetectorOn && range.InRange(w.Timestamp));
        }

        public static int PedBeginWalkCount(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            return events.Count(w => w.EventCode == (short)IndianaEnumerations.PedestrianBeginWalk && range.InRange(w.Timestamp));
        }

        public static int PedCallsRegisteredCount(this IEnumerable<IndianaEvent> events, StartEndRange range)
        {
            return events.Count(w => w.EventCode == (short)IndianaEnumerations.PedestrianCallRegistered && range.InRange(w.Timestamp));
        }

        /// <summary>
        /// Identifies pedestrian cycles from a sequence of <see cref="IndianaEvent"/> objects.
        /// Filters and matches pedestrian detector activations with corresponding walk and change interval events
        /// to construct a list of <see cref="PedCycle"/> instances representing pedestrian service intervals.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to analyze for pedestrian cycles.</param>
        /// <param name="isPedPhaseOverlap">
        /// If <c>true</c>, uses overlap pedestrian event codes for matching cycles; otherwise, uses standard pedestrian event codes.
        /// </param>
        /// <returns>
        /// A read-only list of <see cref="PedCycle"/> objects, each representing a detected pedestrian cycle
        /// with timing information for detector activation, walk start, and change interval.
        /// </returns>
        public static IReadOnlyList<PedCycle> IdentifyPedCycles(this IEnumerable<IndianaEvent> events, bool isPedPhaseOverlap = false)
        {
            var (a, b) = isPedPhaseOverlap
                ? ((short)IndianaEnumerations.PedestrianOverlapBeginWalk, (short)IndianaEnumerations.PedestrianOverlapBeginClearance)
                : ((short)IndianaEnumerations.PedestrianBeginWalk, (short)IndianaEnumerations.PedestrianBeginChangeInterval);

            var filter = events
                .WhereCode([
                    (short)a,
                    (short)b,
                    (short)IndianaEnumerations.PedDetectorOn
                ])
                .OrderBy(o => o.Timestamp)
                .KeepFirstSequentialEvent(IndianaEnumerations.PedDetectorOn)
                .ToList();

            var result = filter
                .Select((t, index) => new { Item = t, Index = index })
                .Where(x => x.Item.EventCode == 90 && x.Index > 0 && x.Index < filter.Count - 1)
                .Select(x =>
                {
                    var prev = filter[x.Index - 1];
                    var next = filter[x.Index + 1];

                    var pedCycle = new PedCycle()
                    {
                        PedDetectorOn = x.Item.Timestamp
                    };

                    if (prev.EventCode == a && next.EventCode == b)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = prev.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    else if (prev.EventCode == a && next.EventCode == a)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = next.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    else if (prev.EventCode == b && next.EventCode == a)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = next.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    return pedCycle;
                })
                .Where(w => w.BeginWalk > DateTime.MinValue)
                .ToList();

            return result;
        }
        
        /// <summary>
        /// Returns a list of <see cref="IndianaEvent"/> where, for the specified <paramref name="eventCode"/>,
        /// only the first occurrence in any sequence of consecutive events with that code is kept.
        /// Other event codes are included as-is.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to filter.</param>
        /// <param name="eventCode">The event code to filter for first-in-sequence occurrences.</param>
        /// <returns>
        /// A list of <see cref="IndianaEvent"/> with only the first event of each consecutive sequence
        /// of the specified event code included.
        /// </returns>
        public static IList<IndianaEvent> KeepFirstSequentialEvent(this IEnumerable<IndianaEvent> events, IndianaEnumerations eventCode)
        {
            var sort = events.OrderBy(o => o.Timestamp).ToList();

            return sort.Where((w, i) => i == 0 || w.EventCode != (int)eventCode || (w.EventCode == (int)eventCode && w.EventCode != sort[i - 1].EventCode)).ToList();
        }

        /// <summary>
        /// Groups a collection of <see cref="IndianaEvent"/> by their parameter type, as determined by the
        /// <see cref="IndianaEventLayerAttribute"/> on the corresponding <see cref="IndianaEnumerations"/> value.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to group.</param>
        /// <returns>
        /// An <see cref="ILookup{TKey, TElement}"/> where the key is the parameter type as a string,
        /// and the value is a collection of <see cref="IndianaEvent"/> with that parameter type.
        /// </returns>
        public static ILookup<string, IndianaEvent> GroupEventsByParamType(this IEnumerable<IndianaEvent> events)
        {
            var codeToParamType = Enum.GetValues(typeof(IndianaEnumerations))
                .Cast<IndianaEnumerations>()
                .ToDictionary(
                    e => (short)e,
                    e => e.GetAttributeOfType<IndianaEventLayerAttribute>()?.IndianaEventParamType.ToString() ?? IndianaEventParamType.None.ToString()
                );

            return events
                .Where(e => codeToParamType.ContainsKey(e.EventCode))
                .ToLookup(e => codeToParamType[e.EventCode]);
        }
    }
}
