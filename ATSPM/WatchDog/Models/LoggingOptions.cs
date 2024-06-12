#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - WatchDog.Services/LoggingOptions.cs
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
namespace WatchDog.Services
{

    public class LoggingOptions
    {
        public DateTime ScanDate { get; set; }
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int ConsecutiveCount { get; set; }
        public int MinPhaseTerminations { get; set; }
        public double PercentThreshold { get; set; }
        public int PreviousDayPMPeakStart { get; set; }
        public int PreviousDayPMPeakEnd { get; set; }
        public int MinimumRecords { get; set; }
        public int LowHitThreshold { get; set; }
        public int MaximumPedestrianEvents { get; set; }
        public bool WeekdayOnly { get; set; }

        public DateTime AnalysisStart
        {
            get
            {
                var startHour = new TimeSpan(ScanDayStartHour, 0, 0);
                return ScanDate.Date + startHour;
            }
        }
        public DateTime AnalysisEnd
        {
            get
            {
                var endHour = new TimeSpan(ScanDayEndHour, 0, 0);
                return ScanDate.Date + endHour;
            }
        }
    }

}