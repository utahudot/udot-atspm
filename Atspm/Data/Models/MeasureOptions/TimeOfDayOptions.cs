#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models.MeasureOptions/TimeOfDayOptions.cs
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

namespace Utah.Udot.Atspm.Data.Models.MeasureOptions
{
    public class TimeOfDayOptions : AtspmOptionsBase
    {
        public const int FixedBinSizeMinutes = 15;

        public List<string> LocationIdentifiers { get; set; } = new();
        public List<DateOnly> SelectedDates { get; set; } = new();
        public int BinSizeMinutes { get; set; } = FixedBinSizeMinutes;
        public TimeOfDayDataSource DataSource { get; set; } = TimeOfDayDataSource.IndianaEvents;
        public List<string> AllDayPrimaryDirections { get; set; } = new();
        public List<string> AmPrimaryDirections { get; set; } = new();
        public List<string> PmPrimaryDirections { get; set; } = new();
        public double AmEntryPctOfPeak { get; set; } = 0.55;
        public double AmExitPctOfPeak { get; set; } = 0.40;
        public double PmEntryPctOfPeak { get; set; } = 0.68;
        public double PmExitPctOfPeak { get; set; } = 0.38;
        public double FreeEntryPctOfDailyPeak { get; set; } = 0.22;
        public double FreeEntryPctOfDynamicRange { get; set; } = 0.18;
        public int EntrySustainedBins { get; set; } = 2;
        public int FreeSustainedBins { get; set; } = 4;
        public string FreeFallbackTime { get; set; } = "23:30";
        public string MaxAmEndTime { get; set; } = "10:00";
        public string MaxPmEndTime { get; set; } = "20:00";
        public double LaneCapacityVehiclesPerHour { get; set; } = 800;
        public double ApproachVolumeAssumedLanes { get; set; } = 2;
        public double SplitReviewThresholdPercent { get; set; } = 35;
        public double ShoulderReviewThresholdPercent { get; set; } = 45;
        public Dictionary<string, double> DirectionLaneCounts { get; set; } = new();
    }

    public enum TimeOfDayDataSource
    {
        IndianaEvents,
        Aggregated
    }
}
