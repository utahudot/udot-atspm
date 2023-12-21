using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Device repository
    /// </summary>
    public interface IDeviceRepository : IAsyncRepository<Device>
    {
        /// <summary>
        /// Gets all <see cref="Device"/> from <paramref name="locationId"/> where <see cref="Device.DeviceStatus"/> equals <see cref="DeviceStatus.Active"/>
        /// </summary>
        /// <param name="locationId">Id of <see cref="Location"/> to get assigned <see cref="Device"/> from</param>
        /// <returns></returns>
        IReadOnlyList<Device> GetActiveDevicesByLocation(int locationId);
    }
}
