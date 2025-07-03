#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Extensions/IDeviceRepositoryExtensions.cs
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

using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IDeviceRepository"/>
    /// </summary>
    public static class IDeviceRepositoryExtensions
    {
        /// <summary>
        /// <see cref="Device"/> query builder for the EventLogUtility.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="queryOptions"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<Device> GetDevicesForLogging(this IDeviceRepository repo, DeviceEventLoggingQueryOptions queryOptions)
        {
            var result = repo.GetList()
                .Where(w => w.LoggingEnabled)
                .Where(w => queryOptions.DeviceType == DeviceTypes.Unknown || w.DeviceType == queryOptions.DeviceType)
                .Where(w => queryOptions.DeviceStatus == DeviceStatus.Unknown || w.DeviceStatus == queryOptions.DeviceStatus)
                .Where(w => queryOptions.TransportProtocol == TransportProtocols.Unknown || w.DeviceConfiguration.Protocol == queryOptions.TransportProtocol)
                .AsQueryable()
                .AsAsyncEnumerable()
                .Where(w => !(queryOptions.IncludedDevices?.Count() > 0) || queryOptions.IncludedDevices.Any(a => w.DeviceIdentifier == a))
                .Where(w => !(queryOptions.IncludeConfigurations?.Count() > 0) || queryOptions.IncludeConfigurations.Any(a => w.DeviceConfigurationId == a))
                .Where(w => !(queryOptions.IncludedLocations?.Count() > 0) || queryOptions.IncludedLocations.Any(a => w.Location.LocationIdentifier == a))
                .Where(w => !(queryOptions.ExcludedLocations?.Count() > 0) || !queryOptions.ExcludedLocations.Any(a => w.Location.LocationIdentifier == a))
                .Where(w => !(queryOptions.IncludedAreas?.Count() > 0) || queryOptions.IncludedAreas.Any(l => w.Location.Areas.Any(a => a.Name == l)))
                .Where(w => !(queryOptions.IncludedRegions?.Count() > 0) || queryOptions.IncludedRegions.Any(a => w.Location.Region.Description == a))
                .Where(w => !(queryOptions.IncludedJurisdictions?.Count() > 0) || queryOptions.IncludedJurisdictions.Any(a => w.Location.Jurisdiction.Name == a))
                .Where(w => !(queryOptions.IncludedLocationTypes?.Count() > 0) || queryOptions.IncludedLocationTypes.Any(a => w.Location.LocationType.Name == a));

            return result;
        }

        /// <summary>
        /// Updates the <see cref="Device.LocationId"/> to new location version
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="deviceIds"></param>
        /// <param name="locationId"></param>
        public static void UpdateDevicesForNewVersion(this IDeviceRepository repo, List<int> deviceIds, int locationId)
        {
            var devices = repo.GetList().Where(w => deviceIds.Contains(w.Id)).ToList();
            foreach (var device in devices)
            {
                device.LocationId = locationId;
                repo.Update(device);
            }
        }
    }
}
