#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/DetectorController.cs
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
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Infrastructure.Attributes;
using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Services;
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
    /// Detector Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class DetectorController(IDetectorRepository repository, IDeviceRepository devices, IEnumerable<IDeviceZoneProvider> zoneProviders) : LocationPolicyControllerBase<Detector, int>(repository)
    {
        private readonly IDetectorRepository _repository = repository;

        #region NavigationProperties

        /// <summary>
        /// <see cref="DetectorComment"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<DetectorComment>> GetDetectorComments([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<DetectorComment>>(key);
        }

        /// <summary>
        /// <see cref="DetectionType"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<DetectionType>> GetDetectionTypes([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<DetectionType>>(key);
        }

        [AuthorizePermission(AtspmAuthorization.Permissions.LocationConfigurationsEdit)]
        [HttpPost("api/v1/Detector/retrieveDetectionData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RetrieveDetectionZoneIdentifierBasedOnDetectionType(ODataActionParameters ourParams)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state.");
            }

            try
            {
                // Reconciliation uses the saved device configuration. Keep the legacy
                // Get Zones request below for the editor's unsaved connection settings.
                if (ourParams.TryGetValue("configurationDeviceId", out var configuredId))
                {
                    if (!int.TryParse(configuredId?.ToString(), out var id))
                        return BadRequest("Invalid configurationDeviceId.");
                    var device = devices.GetList().Include(d => d.DeviceConfiguration).ThenInclude(c => c.Product)
                        .FirstOrDefault(d => d.Id == id);
                    if (device == null) return NotFound("Device not found.");
                    var providers = zoneProviders.Where(p => p.CanRead(device)).ToArray();
                    if (providers.Length != 1)
                        return BadRequest("No unique zone provider is available for this device. The built-in provider supports Econolite Vision FIR/AI cameras; other products require a compatible zone provider.");
                    return Ok(await providers[0].ReadZonesAsync(device, HttpContext.RequestAborted));
                }

                if (!ourParams.ContainsKey("IpAddress") || !ourParams.ContainsKey("port") || !ourParams.ContainsKey("detectionType") || !ourParams.ContainsKey("deviceId"))
                {
                    return BadRequest("Missing required parameters.");
                }

                string ipAddress = ourParams["IpAddress"].ToString();
                string port = ourParams["port"].ToString();
                string deviceId = ourParams["deviceId"].ToString();
                if (!Enum.TryParse(ourParams["detectionType"].ToString(), out DeviceTypes detectionType))
                {
                    return BadRequest("Invalid detectionType value.");
                }

                if (detectionType == DeviceTypes.FIRCamera)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string url = $"http://{ipAddress}:{port}/api/v1/cameras/{deviceId}/zones";
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadFromJsonAsync<CameraZoneDetailsResponse>();
                            List<String> availableCameraZoneNames = responseData.Zones.Select(i => i.Name).ToList();
                            return Ok(availableCameraZoneNames.ToArray());
                            //return Ok(responseData.Zones.ToArray());
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

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}
