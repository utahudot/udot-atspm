#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services/LocationManager.cs
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
using Utah.Udot.Atspm.Infrastructure.Extensions;

namespace Utah.Udot.Atspm.Infrastructure.Services
{
    public interface ILocationManager
    {
        Task<Location> CopyLocationToNewVersion(int key, string newVersionLabel);
        Task SetLocationToDeleted(int key);
        Task DeleteAllVersions(string locationIdentifier);
    }

    public class LocationManager : ILocationManager
    {
        private readonly ILocationRepository _locations;
        private readonly IDeviceRepository _devices;

        public LocationManager(ILocationRepository locations, IDeviceRepository devices)
        {
            _locations = locations ?? throw new ArgumentNullException(nameof(locations));
            _devices = devices ?? throw new ArgumentNullException(nameof(devices));
        }

        public async Task<Location> CopyLocationToNewVersion(int key, string newVersionLabel)
        {
            var deviceIds = _devices.GetList()
                .Where(w => w.LocationId == key)
                .Select(s => s.Id)
                    .ToList();
            var newLocation = await _locations.CopyLocationToNewVersion(key, newVersionLabel);
            _devices.UpdateDevicesForNewVersion(deviceIds, newLocation.Id);
            return newLocation;
        }

        public async Task DeleteAllVersions(string locationIdentifier)
        {
            var versions = _locations.GetList()
                 .Include(i => i.Jurisdiction)
                 .Include(i => i.Region)
                 .Include(i => i.Devices)
                 .FromSpecification(new LocationIdentifierSpecification(locationIdentifier))
                 .FromSpecification(new ActiveLocationSpecification())
                 .ToList();

            foreach (var version in versions)
            {
                if (version.Devices != null)
                {
                    foreach (var device in version.Devices)
                    {
                        _devices.Remove(device);
                    }
                }
                await _locations.SetLocationToDeleted(version.Id);
            }
        }

        public async Task SetLocationToDeleted(int key)
        {
            var location = await _locations.LookupAsync(key);
            var versions = _locations.GetAllVersionsOfLocation(location.LocationIdentifier);
            if (versions.Count() > 1)
            {
                //get the version previous to the one being deleted
                var previousVersion = versions.Where(w => w.Start < location.Start).OrderByDescending(o => o.Start).FirstOrDefault();
                if (previousVersion != null && location.Devices != null)
                {
                    //assign the devices of the deleted version to the location id of the previous version
                    foreach (var device in location.Devices)
                    {
                        device.LocationId = previousVersion.Id;
                    }
                }
            }
            else
            {
                if (location.Devices != null)
                {
                    foreach (var device in location.Devices)
                    {
                        _devices.Remove(device);
                    }
                }
            }
            await _locations.SetLocationToDeleted(key);
        }
    }
}
