﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application/AtspmMath.cs
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

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm
{
    /// <summary>
    /// Equations used in the ATSPM engine as defined by Measures and Assumptions
    /// </summary>
    public static class AtspmMath
    {
        /// <summary>
        /// Calculates the Peak Hour Factor for Approach Volumes
        /// <list type="bullet">
        /// <listheader>Used in the following steps of Approach Volume:</listheader>
        /// <item>
        /// <term>Equation 3</term>
        /// <description>Step 3: Calculate Peak Hour Factor</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="peakHourVolume">Peak vehicle volume hour</param>
        /// <param name="peakBinVolume">Peak vehicle volume per bin</param>
        /// <param name="binSize">Bin size to break up hours</param>
        /// <returns></returns>
        public static double PeakHourFactor(double peakHourVolume, double peakBinVolume, int binSize)
        {
            return Math.Round(peakHourVolume / (binSize * peakBinVolume), 3);
        }

        public static double PeakHourDFactor(double peakHourVolumeA, double peakHourVolumeB)
        {
            return Math.Round(peakHourVolumeA / (peakHourVolumeA + peakHourVolumeB), 3);
        }

        public static double PeakHourKFactor(double totalVolumeA, double peakHourVolumeA, double totalVolumeB, double peakHourVolumeB)
        {
            return Math.Round((peakHourVolumeA + peakHourVolumeB) / (totalVolumeA + totalVolumeB), 3);
        }

        /// <summary>
        /// Calculates the <see cref="TimeSpan"/> difference between the first and second <see cref="IndianaEnumerations"/>
        /// <list type="bullet">
        /// <listheader>Used in the following steps of Preemption Details:</listheader>
        /// <item>
        /// <term>Equation 52</term>
        /// <description>Step 1: Calculate Dwell Time</description>
        /// </item>
        /// <item>
        /// <term>Equation 53</term>
        /// <description>Step 2: Calculate Track Clear Time</description>
        /// </item>
        /// <item>
        /// <term>Equation 54</term>
        /// <description>Step 3: Calculate Time to Service</description>
        /// </item>
        /// <item>
        /// <term>Equation 55</term>
        /// <description>Step 4: Calculate Delay</description>
        /// </item>
        /// <item>
        /// <term>Equation 56</term>
        /// <description>Step 5: Calculate Time to Gate Down</description>
        /// </item>
        /// <item>
        /// <term>Equation 57</term>
        /// <description>Step 6: Calculate Time to Call Max Out</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="items"><see cref="IndianaEvent"/> list the <see cref="IndianaEnumerations"/> are sorted</param>
        /// <param name="first">Starting event code</param>
        /// <param name="second">Ending event code</param>
        /// <returns><see cref="TimeSpan"/> difference of <paramref name="second"/> minus <paramref name="first"/></returns>
        public static IEnumerable<Tuple<IndianaEvent[], TimeSpan>> TimeSpanFromConsecutiveCodes(this IEnumerable<IndianaEvent> items, short first, short second)
        {
            var preFilter = items.OrderBy(o => o.Timestamp)
                .Where(w => w.EventCode == first || w.EventCode == second)
                //.Where(w => w.Timestamp > DateTime.MinValue && w.Timestamp < DateTime.MaxValue)
                .ToList();

            var result = preFilter.Where((x, y) =>
                    y < preFilter.Count - 1 && x.EventCode == first && preFilter[y + 1].EventCode == second ||
                    y > 0 && x.EventCode == second && preFilter[y - 1].EventCode == first)
                        .Chunk(2)
                        .Select(l => new Tuple<IndianaEvent[], TimeSpan>(new IndianaEvent[] { l[0], l[1] }, l[1].Timestamp - l[0].Timestamp));

            return result;
        }

        /// <summary>
        /// Identify and Adjust Vehicle Activations
        /// <list type="bullet">
        /// <listheader>Used in the following Measures and Assumptions:</listheader>
        /// <item>
        /// <term>Equation 1</term>
        /// <description>Used in step 1 of Approach Volume</description>
        /// </item>
        /// <item>
        /// <term>Equation 27</term>
        /// <description>Used in step 1 of Approach Delay</description>
        /// </item>
        /// <item>
        /// <term>Equation 44</term>
        /// <description>Used in step 1 of Purdue Coordination Diagram</description>
        /// </item>
        /// <item>
        /// <term>Equation 64</term>
        /// <description>Used in step 1 of Arrivals on Red</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="timestamp">time stamp of arrival time (EC 82)</param>
        /// <param name="approachSpeed">stated speed for the approach</param>
        /// <param name="distanceFromStopBar">stated distance to stop bar</param>
        /// <param name="latencyCorrection">latency correction, based on system latency factors</param>
        /// <returns>adjusted time stamp of arrival time</returns>
        public static DateTime AdjustTimeStamp(DateTime timestamp, int approachSpeed, int distanceFromStopBar, double latencyCorrection = 0)
        {
            return timestamp.AddSeconds(distanceFromStopBar / (approachSpeed * 1.467)).AddSeconds(latencyCorrection * -1);
        }

        public static IReadOnlyList<T> GetPeakVolumes<T>(this IEnumerable<T> volumes, int chunks) where T : IDetectorCount
        {
            return volumes.Where((w, i) => i <= volumes.Count() - chunks)
                .Select((s, i) => volumes.Skip(i).Take(chunks))
                .Aggregate((a, b) => a.Sum(s => s.DetectorCount) >= b.Sum(s => s.DetectorCount) ? a : b).ToList();
        }

        //HACK: this is not working right!
        public static IReadOnlyList<IndianaEvent> GetLastConsecutiveEvent(this IEnumerable<IndianaEvent> events, int consecutiveCount = 2)
        {
            return events
                .OrderBy(o => o.Timestamp)
                .Skip(consecutiveCount - 1)
                .Where((w, i) => events
                .Skip(i - consecutiveCount)
                .Take(consecutiveCount)
                .All(a => a.EventCode == w.EventCode))
                .ToList();
        }
    }
}
