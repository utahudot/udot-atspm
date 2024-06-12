#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.ConfigurationRepositories/IDeviceRepository.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories.ConfigurationRepositories
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

        /// <summary>
        /// Returns all <see cref="Device"/> from all latest version <see cref="Location"/>
        /// that are also enabled for logging.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<Device> GetActiveDevicesByAllLatestLocations();
    }
}
