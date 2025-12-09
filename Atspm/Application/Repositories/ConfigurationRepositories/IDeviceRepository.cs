#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Repositories.ConfigurationRepositories/IDeviceRepository.cs
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
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Repository interface for accessing and querying <see cref="Device"/> entities.
    /// </summary>
    public interface IDeviceRepository : IAsyncRepository<Device>
    {
        /// <summary>
        /// Retrieves all active <see cref="Device"/> entities assigned to the specified <see cref="Location"/>.
        /// </summary>
        /// <param name="locationId">
        /// The unique identifier of the <see cref="Location"/> whose devices should be retrieved.
        /// </param>
        /// <returns>
        /// A read-only list of <see cref="Device"/> records where <see cref="Device.DeviceStatus"/> equals <see cref="DeviceStatus.Active"/>.
        /// </returns>
        IReadOnlyList<Device> GetActiveDevicesByLocation(int locationId);

        /// <summary>
        /// Retrieves all active <see cref="Device"/> entities from the latest version of all <see cref="Location"/> records
        /// that are also enabled for logging.
        /// </summary>
        /// <returns>
        /// A read-only list of <see cref="Device"/> records that are active and logging-enabled.
        /// </returns>
        IReadOnlyList<Device> GetActiveDevicesByAllLatestLocations();

        /// <summary>
        /// Determines whether a <see cref="Device"/> exists for the specified identifier.
        /// </summary>
        /// <param name="deviceId">The unique identifier of the device.</param>
        /// <returns>
        /// A task that resolves to <c>true</c> if the device exists; otherwise <c>false</c>.
        /// </returns>
        Task<bool> DeviceExists(int deviceId);
    }

}
