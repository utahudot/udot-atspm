#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Watchdog/WatchdogEmailRecipient.cs
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
    public class WatchdogEmailRecipient
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsWatchdogSubscriber { get; set; }
        public List<int> RegionIds { get; set; } = new();
        public List<int> JurisdictionIds { get; set; } = new();
        public List<int> AreaIds { get; set; } = new();

        public bool CanReceiveAllLocationsEmail => IsAdmin && IsWatchdogSubscriber;
    }
}
