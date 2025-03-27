#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/DeviceController.cs
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

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.ConfigApi.Models;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Device controller
    /// </summary>
    [ApiVersion(1.0)]
    public class DeviceController : LocationPolicyControllerBase<Device, int>
    {
        private readonly IDeviceRepository _repository;

        /// <inheritdoc/>
        public DeviceController(IDeviceRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        #endregion

        #region Actions

        #endregion

        #region Functions

        /// <summary>
        /// Gets all <see cref="Device"/> from <paramref name="locationId"/> where <see cref="Device.DeviceStatus"/> equals <see cref="DeviceStatus.Active"/>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<Device>), Status200OK)]
        public IActionResult GetActiveDevicesByLocation(int locationId)
        {
            return Ok(_repository.GetActiveDevicesByLocation(locationId));
        }

        /// <summary>
        /// Gets a count of device type for all active devices <see cref="Device"/> where <see cref="Device.DeviceStatus"/> equals <see cref="DeviceStatus.Active"/>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<DeviceGroup>), Status200OK)]
        public IActionResult GetActiveDevicesCount()
        {
            var devices = _repository.GetActiveDevicesByAllLatestLocations();
            var deviceGroups = devices.GroupBy(d => new
            {
                Manufacturer = d.DeviceConfiguration.Product.Manufacturer,
                Model = d.DeviceConfiguration.Product.Model,
                Firmware = d.DeviceConfiguration.Description
            })
                .Select(g => new ATSPM.ConfigApi.Models.DeviceGroup
                {
                    Manufacturer = g.Key.Manufacturer,
                    Model = g.Key.Model,
                    Firmware = g.Key.Firmware,
                    Count = g.Count()
                })
                .ToList();

            return Ok(deviceGroups);
        }

        #endregion
    }
}
