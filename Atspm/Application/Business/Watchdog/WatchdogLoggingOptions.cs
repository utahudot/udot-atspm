#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Watchdog/WatchdogLoggingOptions.cs
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

namespace Utah.Udot.Atspm.Business.Watchdog
{
    //TODO: this needs to be added into WatchdogConfiguration and config json needs to be changed from flat
    public class WatchdogLoggingOptions
    {
        public DateTime PmScanDate { get; set; }
        public DateTime AmScanDate { get; set; }
        public DateTime RampMissedDetectorHitsStartScanDate { get; set; }
        public DateTime RampMissedDetectorHitsEndScanDate { get; set; }
        public int AmStartHour { get; set; }
        public int AmEndHour { get; set; }
        public int PmPeakStartHour { get; set; }
        public int PmPeakEndHour { get; set; }
        public int RampDetectorStartHour { get; set; }
        public int RampDetectorEndHour { get; set; }
        public int RampMissedDetectorHitStartHour { get; set; }
        public int RampMissedDetectorHitEndHour { get; set; }
        public int RampMainlineStartHour { get; set; }
        public int RampMainlineEndHour { get; set; }
        public int RampStuckQueueStartHour { get; set; }
        public int RampStuckQueueEndHour { get; set; }
        public bool WeekdayOnly { get; set; }
        public int ConsecutiveCount { get; set; }
        public int MinPhaseTerminations { get; set; }
        public double PercentThreshold { get; set; }
        public int MinimumRecords { get; set; }
        public int LowHitThreshold { get; set; }
        public int LowHitRampThreshold { get; set; }
        public int MaximumPedestrianEvents { get; set; }
        public int RampMissedEventsThreshold { get; set; }
    }

    public class WatchdogPmLoggingOptions
    {
        public DateTime PmScanDate { get; set; }
        public int PmPeakStartHour { get; set; }
        public int PmPeakEndHour { get; set; }
        public int RampMainlineStartHour { get; set; }
        public int RampMainlineEndHour { get; set; }
        public int RampStuckQueueStartHour { get; set; }
        public int RampStuckQueueEndHour { get; set; }
        public int MinimumRecords { get; set; }
        public int LowHitThreshold { get; set; }

        public DateTime PmAnalysisStart => PmScanDate.Date + new TimeSpan(PmPeakStartHour, 0, 0);
        public DateTime PmAnalysisEnd => PmScanDate.Date + new TimeSpan(PmPeakEndHour, 0, 0);
    }

    public class WatchdogAmLoggingOptions
    {
        public DateTime AmScanDate { get; set; }
        public int AmStartHour { get; set; }
        public int AmEndHour { get; set; }
        public int ConsecutiveCount { get; set; }
        public int MinPhaseTerminations { get; set; }
        public double PercentThreshold { get; set; }
        public int MaximumPedestrianEvents { get; set; }

        public DateTime AmAnalysisStart => AmScanDate.Date + new TimeSpan(AmStartHour, 0, 0);
        public DateTime AmAnalysisEnd => AmScanDate.Date + new TimeSpan(AmEndHour, 0, 0);
    }

    public class WatchdogRampLoggingOptions
    {
        public DateTime RampMissedDetectorHitsStartScanDate { get; set; }
        public DateTime RampMissedDetectorHitsEndScanDate { get; set; }
        public int RampMissedDetectorHitStartHour { get; set; }
        public int RampMissedDetectorHitEndHour { get; set; }
        public int RampMissedEventsThreshold { get; set; }
        public int RampDetectorStartHour { get; set; }
        public int RampDetectorEndHour { get; set; }
        public int LowHitRampThreshold { get; set; }

        public DateTime RampMissedDetectorHitStart => RampMissedDetectorHitsStartScanDate.Date + new TimeSpan(RampMissedDetectorHitStartHour, 0, 0);
        public DateTime RampMissedDetectorHitEnd => RampMissedDetectorHitsEndScanDate.Date + new TimeSpan(RampMissedDetectorHitEndHour, 0, 0);
        public DateTime RampDetectorStart => RampMissedDetectorHitsStartScanDate.Date + new TimeSpan(RampDetectorStartHour, 0, 0);
        public DateTime RampDetectorEnd => RampMissedDetectorHitsStartScanDate.Date + new TimeSpan(RampDetectorEndHour, 0, 0);
    }
}
