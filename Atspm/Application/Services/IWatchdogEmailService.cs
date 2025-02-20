#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services/IWatchdogEmailService.cs
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

using Utah.Udot.Atspm.Business.Watchdog;

namespace Utah.Udot.Atspm.Services
{
    public interface IWatchdogEmailService
    {
        Task SendAllEmails(
            WatchdogEmailOptions options,
            List<WatchDogLogEventWithCountAndDate> newErrors,
            List<WatchDogLogEventWithCountAndDate> dailyRecurringErrors,
            List<WatchDogLogEventWithCountAndDate> recurringErrors,
            List<Location> Locations,
            List<ApplicationUser> users,
            List<Jurisdiction> jurisdictions,
            List<UserJurisdiction> userJurisdictions,
            List<Area> areas,
            List<UserArea> userAreas,
            List<Region> regions,
            List<UserRegion> userRegions,
            List<WatchDogLogEvent> logsFromPreviousDay);

        Task<string> CreateEmailBody(
            WatchdogEmailOptions emailOptions,
            List<WatchDogLogEventWithCountAndDate> newErrors,
            List<WatchDogLogEventWithCountAndDate> dailyRecurringErrors,
            List<WatchDogLogEventWithCountAndDate> recurringErrors,
            List<Location> locations,
            List<WatchDogLogEvent> logsFromPreviousDay);
    }
}
