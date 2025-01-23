#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/IDeviceRepositoryExtensions.cs
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
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.ValueObjects;

namespace Utah.Udot.Atspm.Extensions
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
            var result = repo.GetList();
            if (queryOptions.IncludedDevices?.Count() > 0)
            {
                result = result.Where(w => queryOptions.IncludedDevices.Any(a => w.Id.ToString() == a));
            }
            var result2 = result.Where(w => w.LoggingEnabled)
                .Where(w => queryOptions.DeviceType == DeviceTypes.Unknown ? true : w.DeviceType == queryOptions.DeviceType)
                .Where(w => queryOptions.DeviceStatus == DeviceStatus.Unknown ? true : w.DeviceStatus == queryOptions.DeviceStatus)
                .Where(w => queryOptions.TransportProtocol == TransportProtocols.Unknown ? true : w.DeviceConfiguration.Protocol == queryOptions.TransportProtocol)
                .AsQueryable()
                .AsAsyncEnumerable()
                .Where(w => (queryOptions.IncludedLocations?.Count() > 0) ? queryOptions.IncludedLocations.Any(a => w.Location.LocationIdentifier == a) : true)
                .Where(w => (queryOptions.ExcludedLocations?.Count() > 0) ? !queryOptions.ExcludedLocations.Any(a => w.Location.LocationIdentifier == a) : true)
                .Where(w => (queryOptions.IncludedAreas?.Count() > 0) ? queryOptions.IncludedAreas.Any(l => w.Location.Areas.Any(a => a.Name == l)) : true)
                .Where(w => (queryOptions.IncludedRegions?.Count() > 0) ? queryOptions.IncludedRegions.Any(a => w.Location.Region.Description == a) : true)
                .Where(w => (queryOptions.IncludedJurisdictions?.Count() > 0) ? queryOptions.IncludedJurisdictions.Any(a => w.Location.Jurisdiction.Name == a) : true)
                .Where(w => (queryOptions.IncludedLocationTypes?.Count() > 0) ? queryOptions.IncludedLocationTypes.Any(a => w.Location.LocationType.Name == a) : true);

            return result2;
        }
    }
}
