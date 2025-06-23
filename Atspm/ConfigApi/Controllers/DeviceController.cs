#region license
// Copyright 2026 Utah Departement of Transportation
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
using Microsoft.AspNetCore.OData.Formatter;
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
    public class DeviceController(IDeviceRepository repository) : DevicePolicyControllerBase<Device, int>(repository)
    {
        private readonly IDeviceRepository _repository = repository;

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
                //HACK: This is a dto not a model
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

        [HttpPost("api/v1/Device/retrieveDeviceData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RetrieveDetectionIdentifierBasedOnDetectionType(ODataActionParameters ourParams)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state.");
            }

            try
            {
                if (!ourParams.ContainsKey("IpAddress") || !ourParams.ContainsKey("port") || !ourParams.ContainsKey("detectionType"))
                {
                    return BadRequest("Missing required parameters.");
                }

                string ipAddress = ourParams["IpAddress"].ToString();
                string port = ourParams["port"].ToString();
                if (!Enum.TryParse(ourParams["detectionType"].ToString(), out DeviceTypes detectionType))
                {
                    return BadRequest("Invalid detectionType value.");
                }

                if (detectionType == DeviceTypes.FIRCamera)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string url = $"http://{ipAddress}:{port}/api/v1/cameras";
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadFromJsonAsync<CameraDetailsResponse>();
                            List<String> availableCameraIds = responseData.Cameras.Select(i => i.Index).ToList();
                            Dictionary<String, String> cameraDictionary = new Dictionary<String, String>();

                            foreach (var cameraId in availableCameraIds)
                            {
                                string urlDeviceInfo = $"http://{ipAddress}:{port}/api/v1/cameras/{cameraId}/device-info";
                                HttpResponseMessage responseDeviceInfo = await client.GetAsync(urlDeviceInfo);

                                if (responseDeviceInfo.IsSuccessStatusCode)
                                {
                                    var responseObject = await responseDeviceInfo.Content.ReadFromJsonAsync<CameraSpecificDetailsInfo>();
                                    String cameraName = responseObject.Name;
                                    cameraDictionary.Add(cameraName, cameraId);
                                }
                                else
                                {
                                    var deviceId = responseData.Cameras.Where(boo => boo.Index.Equals(cameraId)).Select(thing => thing.DeviceId).First();
                                    cameraDictionary.Add($"Camera-{deviceId}", cameraId);
                                }
                            }

                            return Ok(cameraDictionary);
                        }
                        return StatusCode((int)response.StatusCode, "Failed to retrieve data from the external service.");
                    }
                }

                return Ok("Detection type not supported yet.");
            }
            catch (HttpRequestException e)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"External service request failed: {e.Message}");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {e.Message}");
            }
        }

        #endregion
    }
}
