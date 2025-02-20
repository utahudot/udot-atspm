#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/WatchdogConfiguration.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class WatchdogConfiguration
    {
        public DateTime ScanDate { get; set; } = DateTime.Today.AddDays(-1);
        public int PreviousDayPMPeakStart { get; set; } = 18;
        public int PreviousDayPMPeakEnd { get; set; } = 17;
        public bool WeekdayOnly { get; set; } = true;
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int ConsecutiveCount { get; set; } = 3;
        public int LowHitThreshold { get; set; } = 50;
        public int MaximumPedestrianEvents { get; set; } = 200;
        public int MinimumRecords { get; set; } = 500;
        public int MinPhaseTerminations { get; set; } = 50;
        public double PercentThreshold { get; set; } = .9;

        public bool EmailAllErrors { get; set; }
        public string DefaultEmailAddress { get; set; }

        public DateTime AnalysisStart => ScanDate.Date + new TimeSpan(ScanDayStartHour, 0, 0);
        public DateTime AnalysisEnd => ScanDate.Date + new TimeSpan(ScanDayEndHour, 0, 0);

        public string Sort { get; set; }
    }
}
