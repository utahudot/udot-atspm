using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Infrastructure.Services
{
    public interface ILocationManager
    {
        Task CopyLocationToNewVersion(int key);
        Task SetLocationToDeleted(int key);
        Task DeleteAllVersions(string locationIdentifier);
    }

    public class LocationManager : ILocationManager
    {
        private readonly ILocationRepository _locations;
        private readonly IDeviceRepository _devices;

        public LocationManager(ILocationRepository locations, IDeviceRepository devices)
        {
            
        }

        public Task CopyLocationToNewVersion(int key)
        {
            var deviceIds = _devices.GetList()
                .Where(w => w.LocationId == key)
                .Select(s => s.Id)
                    .ToList();
            var newLocation = await _repository.CopyLocationToNewVersion(key);
            _deviceRepository.UpdateDevicesForNewVersion(deviceIds, newLocation.Id);
        }

        public Task DeleteAllVersions(string locationIdentifier)
        {
            try
            {
                var versions = _repository.GetAllVersionsOfLocationWithDevices(key);
                foreach (var version in versions)
                {
                    if (version.Devices != null)
                    {
                        foreach (var device in version.Devices)
                        {
                            _deviceRepository.Remove(device);
                        }
                    }
                    await _repository.SetLocationToDeleted(version.Id);
                }
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }

        public Task SetLocationToDeleted(int key)
        {
            try
            {
                var location = await _repository.LookupAsync(key);
                var versions = _repository.GetAllVersionsOfLocation(location.LocationIdentifier);
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
                            _deviceRepository.Remove(device);
                        }
                    }
                }
                await _repository.SetLocationToDeleted(key);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }
    }

    public class Temp
    {
        //IReadOnlyList<Location> GetAllVersionsOfLocationWithDevices(string LocationIdentifier);
        //Location GetLatestVersionOfLocationWithDevice(string LocationIdentifier, DateTime startDate);

        /// <inheritdoc/>
        public IReadOnlyList<Location> GetAllVersionsOfLocationWithDevices(string LocationIdentifier)
        {
            var result = BaseQuery()
                .Include(i => i.Devices)
                .FromSpecification(new LocationIdSpecification(LocationIdentifier))
                .FromSpecification(new ActiveLocationSpecification())
                .ToList();

            return result;
        }


        /// <inheritdoc/>
        public Location GetLatestVersionOfLocationWithDevice(string LocationIdentifier, DateTime startDate)
        {
            var result = BaseQuery()
                .Include(l => l.Devices).ThenInclude(d => d.DeviceConfiguration).ThenInclude(d => d.Product)
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .FromSpecification(new LocationIdSpecification(LocationIdentifier))
                .Where(Location => Location.Start <= startDate)
                .FromSpecification(new ActiveLocationSpecification())
                .FirstOrDefault();

            return result;
        }
    }
}
