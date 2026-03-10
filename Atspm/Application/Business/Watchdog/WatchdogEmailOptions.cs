#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Watchdog/WatchdogEmailOptions.cs
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
    public class WatchdogEmailOptions
    {
        //public DateTime EmailScanDate { get; set; }
        public DateTime PmScanDate { get; set; }
        public DateTime AmScanDate { get; set; }
        public DateTime RampMissedDetectorHitsStartScanDate { get; set; }
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

        public bool EmailAllErrors { get; set; }
        public bool EmailAmErrors { get; set; }
        public bool EmailPmErrors { get; set; }
        public bool EmailRampErrors { get; set; }
        public string DefaultEmailAddress { get; set; }
        public bool WeekdayOnly { get; set; }
        public string Sort { get; set; }
    }

    public enum WatchdogScanType
    {
        Pm,
        Am,
        Ramp
    }
}
