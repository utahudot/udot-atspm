#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TurningMovementCounts/TurningMovementCountsStatistics.cs
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

namespace Utah.Udot.Atspm.Business.TurningMovementCounts
{
    /// <summary>
    /// Computes peak hours on the selected bin boundaries, using minute counts for 15-minute PHF.
    /// </summary>
    public static class TurningMovementCountsStatistics
    {
        public static (KeyValuePair<DateTime, int>? PeakHour, double? PeakHourFactor) Calculate(
            IReadOnlyList<DataPointForInt> minuteVolumes, DateTime start, DateTime end, int binSize)
        {
            if (binSize <= 0 || binSize > 60 || 60 % binSize != 0)
                throw new ArgumentOutOfRangeException(nameof(binSize));

            if (end - start < TimeSpan.FromHours(1))
                return (null, null);

            var minutes = minuteVolumes
                .Where(v => v.Timestamp >= start && v.Timestamp < end)
                .GroupBy(v => v.Timestamp)
                .Select(g => new DataPointForInt(g.Key, g.Sum(v => v.Value)))
                .OrderBy(v => v.Timestamp)
                .ToList();
            var sums = new int[minutes.Count + 1];
            for (var i = 0; i < minutes.Count; i++)
                sums[i + 1] = sums[i] + minutes[i].Value;

            var bestIndex = -1;
            var bestSum = 0;
            for (var i = 0; i + 60 <= minutes.Count; i++)
            {
                var candidate = minutes[i].Timestamp;
                if ((candidate - start).Ticks % TimeSpan.FromMinutes(binSize).Ticks != 0 ||
                    candidate.AddHours(1) > end ||
                    minutes[i + 59].Timestamp != candidate.AddMinutes(59))
                    continue;

                var count = sums[i + 60] - sums[i];
                if (count > bestSum)
                {
                    bestSum = count;
                    bestIndex = i;
                }
            }

            // Zero traffic has no meaningful peak hour or peak-hour factor.
            if (bestIndex < 0)
                return (null, null);

            var peakQuarter = Enumerable.Range(0, 4)
                .Max(q => sums[bestIndex + (q + 1) * 15] - sums[bestIndex + q * 15]);
            return (new KeyValuePair<DateTime, int>(minutes[bestIndex].Timestamp, bestSum),
                Math.Round(bestSum / (4.0 * peakQuarter), 2));
        }
    }
}
