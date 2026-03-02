#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/ApplicationUser.cs
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

namespace Utah.Udot.Atspm.Data.Models.AggregationModels
{
    public class PedatLocationData
    {
        public string LocationIdentifier { get; set; }
        public string Names { get; set; }
        public string Areas { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double TotalVolume { get; set; }
        public double AverageDailyVolume { get; set; }                           // add daily
        public List<IndexedVolume> AverageVolumeByHourOfDay { get; set; }     // changed   
        public List<IndexedVolume> AverageVolumeByDayOfWeek { get; set; }     // changed
        public List<IndexedVolume> AverageVolumeByMonthOfYear { get; set; }   // changed
        public List<RawDataPoint> RawData { get; set; }                       // should this be aggregate instead of raw? idk
        public StatisticsDataPoint? StatisticData { get; set; } = null;
    }

    public class IndexedVolume
    {
        public int Index { get; set; }
        public double Volume { get; set; }
    }

    public class PedatLocationDataQuery
    {
        public List<string> LocationIdentifiers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PedestrianTimeUnit TimeUnit { get; set; }  // e.g., "HOUR", "DAY", "WEEK", "MONTH", "YEAR"
        public int? Phase { get; set; }       // nullable → all if null
    }

    public class RawDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double? PedestrianCount { get; set; }
    }

    public enum PedestrianTimeUnit
    {
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    public class StatisticsDataPoint
    {
        public double? Events { get; set; }
        public double? Count { get; set; }
        public double? Mean { get; set; }
        public double? Std { get; set; }
        public double? Min { get; set; }
        public double? TwentyFifthPercentile { get; set; }
        public double? FiftiethPercentile { get; set; }
        public double? SeventyFifthPercentile { get; set; }
        public double? Max { get; set; }
        public double? MissingCount { get; set; }
    }
}