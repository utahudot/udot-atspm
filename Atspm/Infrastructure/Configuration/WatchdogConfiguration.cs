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
        public DateTime PmScanDate { get; set; } = DateTime.Today.AddDays(-1);
        public DateTime AmScanDate { get; set; } = DateTime.Today;
        public DateTime RampMissedDetectorHitsStartScanDate { get; set; } = DateTime.Today.AddDays(-1);
        public DateTime RampMissedDetectorHitsEndScanDate { get; set; } = DateTime.Today;

        public int AmStartHour { get; set; } = 1;
        public int AmEndHour { get; set; } = 5;
        public int PmPeakStartHour { get; set; } = 18;
        public int PmPeakEndHour { get; set; } = 17;
        public int RampDetectorStartHour { get; set; } = 7;
        public int RampDetectorEndHour { get; set; } = 8;
        public int RampMissedDetectorHitStartHour { get; set; } = 15;
        public int RampMissedDetectorHitEndHour { get; set; } = 7;
        public int RampMainlineStartHour { get; set; } = 15;
        public int RampMainlineEndHour { get; set; } = 19;
        public int RampStuckQueueStartHour { get; set; } = 1;
        public int RampStuckQueueEndHour { get; set; } = 4;

        public bool WeekdayOnly { get; set; } = true;
        public int ConsecutiveCount { get; set; } = 3;
        public int MinPhaseTerminations { get; set; } = 50;
        public double PercentThreshold { get; set; } = .9;
        public int MinimumRecords { get; set; } = 500;
        public int LowHitThreshold { get; set; } = 50;
        public int LowHitRampThreshold { get; set; } = 10;
        public int MaximumPedestrianEvents { get; set; } = 200;
        public int RampMissedEventsThreshold { get; set; } = 3;

        public bool EmailAllErrors { get; set; }
        public bool EmailPmErrors { get; set; } = true;
        public bool EmailAmErrors { get; set; } = true;
        public bool EmailRampErrors { get; set; } = true;
        public string DefaultEmailAddress { get; set; }

        public string Sort { get; set; }
    }
}
