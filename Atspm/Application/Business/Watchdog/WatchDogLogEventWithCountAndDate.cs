#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Watchdog/WatchDogLogEventWithCountAndDate.cs
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

namespace Utah.Udot.Atspm.Business.Watchdog
{
    public class WatchDogLogEventWithCountAndDate : WatchDogLogEvent
    {
        public WatchDogLogEventWithCountAndDate(int locationId, string locationIdentifier, DateTime timestamp, WatchDogComponentTypes componentType, int componentId, WatchDogIssueTypes issueType, string details, int? phase) : base(locationId, locationIdentifier, timestamp, componentType, componentId, issueType, details, phase)
        {
        }

        public int EventCount { get; set; }
        public DateTime DateOfFirstInstance { get; set; }
        public int ConsecutiveOccurenceCount { get; set; }
    }
}
