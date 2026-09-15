#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayDtos.cs
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

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public class TimeOfDayResult
    {
        public List<string> LocationIdentifiers { get; set; } = new();
        public List<DateOnly> SelectedDates { get; set; } = new();
        public int BinSizeMinutes { get; set; }
        public string DataSource { get; set; } = string.Empty;
        public TimeOfDayRecommendationDto Recommendation { get; set; } = new();
        public TimeOfDayPlanProfileDto PlanProfile { get; set; } = new();
        public TimeOfDaySplitPressureDto SplitPressure { get; set; } = new();
        public TimeOfDayPlanComparisonDto PlanComparison { get; set; } = new();
        public List<TimeOfDayLocationResult> Locations { get; set; } = new();
        public List<TimeOfDayWarningDto> Warnings { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }

    public class TimeOfDayPlanProfileDto
    {
        public TimeOfDayProfileDto CorridorProfile { get; set; } = new();
        public List<TimeOfDayProfileDto> DirectionalProfiles { get; set; } = new();
        public List<TimeOfDayPeakEventDto> Peaks { get; set; } = new();
    }

    public class TimeOfDayProfileDto
    {
        public string Label { get; set; } = string.Empty;
        public string Units { get; set; } = "vph";
        public string Direction { get; set; } = string.Empty;
        public string Movement { get; set; } = string.Empty;
        public string MovementLabel { get; set; } = string.Empty;
        public List<TimeOfDayProfilePointDto> Points { get; set; } = new();
    }

    public class TimeOfDayProfilePointDto
    {
        public string TimeOfDay { get; set; } = string.Empty;
        public int Minutes { get; set; }
        public double AverageVolume { get; set; }
        public double SmoothedVolume { get; set; }
        public double? RollingHourVph { get; set; }
        public double Delta { get; set; }
        public int? ParticipatingLocations { get; set; }
    }

    public class TimeOfDayPeakEventDto
    {
        public string Label { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string LocationIdentifier { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public string TimeOfDay { get; set; } = string.Empty;
        public int Minutes { get; set; }
        public double Value { get; set; }
        public string ValueUnits { get; set; } = string.Empty;
    }

    public class TimeOfDaySplitPressureDto
    {
        public List<string> PrimaryDirections { get; set; } = new();
        public List<string> CrossDirections { get; set; } = new();
        public TimeOfDayProfileDto PrimaryProfile { get; set; } = new();
        public TimeOfDayProfileDto CrossStreetProfile { get; set; } = new();
        public List<TimeOfDayCrossTrafficSharePointDto> CrossTrafficShare { get; set; } = new();
        public Dictionary<string, double> ThresholdPercentByName { get; set; } = new();
        public List<TimeOfDayPeakEventDto> PeriodPeaks { get; set; } = new();
        public List<TimeOfDayCrossTrafficLocationDto> CrossTrafficLocations { get; set; } = new();
        public List<TimeOfDayMovementPressureDto> MovementPressures { get; set; } = new();
        public double? PrimaryPeakVolume { get; set; }
        public string PrimaryPeakTime { get; set; } = string.Empty;
        public double? CrossStreetPeakVolume { get; set; }
        public string CrossStreetPeakTime { get; set; } = string.Empty;
        public double? PeakCrossTrafficPercent { get; set; }
        public string PeakCrossTrafficPercentTime { get; set; } = string.Empty;
        public bool PrimaryStreetRemainsDominant { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public string ReviewText { get; set; } = string.Empty;
    }

    public class TimeOfDayCrossTrafficSharePointDto
    {
        public string TimeOfDay { get; set; } = string.Empty;
        public int Minutes { get; set; }
        public double PrimaryVolume { get; set; }
        public double CrossStreetVolume { get; set; }
        public double TotalVolume { get; set; }
        public double? CrossTrafficPercent { get; set; }
    }

    public class TimeOfDayCrossTrafficLocationDto
    {
        public string LocationIdentifier { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string PeakTime { get; set; } = string.Empty;
        public int Minutes { get; set; }
        public double TotalVehiclesPerHour { get; set; }
        public double? PercentOfCrossTraffic { get; set; }
    }

    public class TimeOfDayRecommendationDto
    {
        public List<Plan> RecommendedSchedule { get; set; } = new();
        public string AmPeakTime { get; set; } = string.Empty;
        public string MiddayValleyTime { get; set; } = string.Empty;
        public string PmPeakTime { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public string AlgorithmVersion { get; set; } = string.Empty;
        public string ThresholdConfigurationName { get; set; } = string.Empty;
    }

    public class TimeOfDayPlanComparisonDto
    {
        public List<Plan> CommonCurrentSchedule { get; set; } = new();
        public List<string> ExceptionLocationIdentifiers { get; set; } = new();
        public string SummaryText { get; set; } = string.Empty;
        public string ExceptionsText { get; set; } = string.Empty;
    }

    public class TimeOfDayLocationResult
    {
        public string LocationIdentifier { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public int DaysWithData { get; set; }
        public List<DateOnly> DatesWithData { get; set; } = new();
        public List<DateOnly> MissingDates { get; set; } = new();
        public bool CoverageFallbackUsed { get; set; }
        public TimeOfDayProfileDto Profile { get; set; } = new();
        public List<TimeOfDayProfileDto> MovementProfiles { get; set; } = new();
        public TimeOfDayLocationSummaryDto Summary { get; set; } = new();
        public List<Plan> CurrentPlanSchedule { get; set; } = new();
        public string DataQualityFlag { get; set; } = string.Empty;
    }

    public class TimeOfDayLocationSummaryDto
    {
        public double? PeakRawVolume { get; set; }
        public double? PeakSmoothedVolume { get; set; }
        public double? PeakHourlyRate { get; set; }
        public double? PeakOccupancyPercent { get; set; }
        public double? AmPeakOccupancyPercent { get; set; }
        public double? PmPeakOccupancyPercent { get; set; }
        public string AmDirectionExceptionMessage { get; set; } = string.Empty;
        public string PmDirectionExceptionMessage { get; set; } = string.Empty;
        public string CrossTrafficReview { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class TimeOfDayMovementPressureDto
    {
        public string Period { get; set; } = string.Empty;
        public string LocationIdentifier { get; set; } = string.Empty;
        public string Movement { get; set; } = string.Empty;
        public string MovementLabel { get; set; } = string.Empty;
        public string PeakTime { get; set; } = string.Empty;
        public double Volume { get; set; }
    }

    public class TimeOfDayWarningDto
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string LocationIdentifier { get; set; } = string.Empty;
    }
}
