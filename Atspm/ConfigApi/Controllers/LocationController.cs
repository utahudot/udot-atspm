#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/LocationController.cs
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.ConfigApi.Models;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Infrastructure.Services;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Location Controller
    /// </summary>
    /// 
    [ApiVersion(1.0)]
    public class LocationController : LocationPolicyControllerBase<Location, int>
    {
        private readonly ILocationRepository _repository;
        private readonly ILocationManager _locationManager;

        //HACK: ILocationManager is temporary

        /// <inheritdoc/>
        public LocationController(ILocationRepository repository, ILocationManager locationManager) : base(repository)
        {
            _repository = repository;
            _locationManager = locationManager;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="Approach"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //[Authorize(Policy = "CanViewLocationConfigurations")]
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Approach>> GetApproaches([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Approach>>(key);
        }

        /// <summary>
        /// <see cref="Area"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //[Authorize(Policy = "CanViewLocationConfigurations")]
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Area>> GetAreas([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Area>>(key);
        }

        /// <summary>
        /// <see cref="Device"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //[Authorize(Policy = "CanViewLocationConfigurations")]
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Device>> GetDevices([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Device>>(key);
        }

        #endregion

        #region Actions

        //HACK: move this to LocationManagementController

        /// <summary>
        /// Copies <see cref="Location"/> and associated <see cref="Approach"/> to new version
        /// </summary>
        /// <param name="key">Location version to copy</param>
        /// <param name="newVersionLabel">Label of new version</param>
        /// <returns>New version of copied <see cref="Location"/></returns>
        /// 
        [Authorize(Policy = "CanEditLocationConfigurations")]
        [HttpPost]
        [ProducesResponseType(typeof(Location), Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CopyLocationToNewVersion(int key, string newVersionLabel)
        {
            try
            {
                var copiedVersion = await _locationManager.CopyLocationToNewVersion(key, newVersionLabel);

                return Ok(copiedVersion);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        /// <summary>
        /// Marks <see cref="Location"/> to deleted
        /// </summary>
        /// <param name="key">Key of <see cref="Location"/> to mark as deleted</param>
        /// <returns></returns>
        /// 
        [Authorize(Policy = "CanDeleteLocationConfigurations")]
        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> SetLocationToDeleted(int key)
        {
            try
            {
                await _locationManager.SetLocationToDeleted(key);

                return Ok();
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        /// <summary>
        /// Marks <see cref="Location"/> to deleted
        /// </summary>
        /// <param name="key">Identifier of <see cref="Location"/> to mark as deleted</param>
        /// <returns></returns>
        /// 
        [Authorize(Policy = "CanDeleteLocationConfigurations")]
        [HttpPost("/api/v1/Location/{key}/DeleteAllVersions")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> DeleteAllVersions(string key)
        {
            try
            {
                await _locationManager.DeleteAllVersions(key);

                return Ok();
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Get latest version of <see cref="Location"/> and related entities that match <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">Location controller identifier</param>
        /// <returns>Lastest <see cref="Location"/> version</returns>
        //[Authorize(Policy = "CanViewLocationConfigurations")]
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Expand | Select, MaxExpansionDepth = 4)]
        [ProducesResponseType(typeof(Location), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public IActionResult GetLatestVersionOfLocation(string identifier)
        {
            //https://learn.microsoft.com/en-us/odata/webapi-8/fundamentals/query-options?tabs=net60
            //options.Request.ODataFeature().SelectExpandClause = new SelectExpandQueryOption(null, "Approaches", options.Context, new ODataQueryOptionParser(
            //    model: options.Context.Model,
            //    targetEdmType: options.Context.NavigationSource.EntityType(),
            //    targetNavigationSource: options.Context.NavigationSource,
            //    queryOptions: new Dictionary<string, string>
            //    {
            //        { "$expand", "Approaches, Areas" }
            //    },
            //    container: options.Context.RequestContainer)).SelectExpandClause;

            var result = _repository.GetLatestVersionOfLocation(identifier);

            if (result == null)
            {
                return NotFound(identifier);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all active <see cref="Location"/> that match <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">Location controller identifier</param>
        /// <returns>List of <see cref="Location"/> in decescing order of start date</returns>
        //[Authorize(Policy = "CanViewLocationConfigurations")]
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<Location>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult GetAllVersionsOfLocation(string identifier)
        {
            var result = _repository.GetAllVersionsOfLocation(identifier);

            if (!result.Any())
            {
                return NotFound(identifier);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get latest version of all <see cref="Location"/>
        /// </summary>
        /// <returns>List of <see cref="Location"/> with newest start date</returns>
        //[Authorize(Policy = "CanViewLocationConfigurations")]
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<Location>), Status200OK)]
        public IActionResult GetLatestVersionOfAllLocations()
        {
            return Ok(_repository.GetLatestVersionOfAllLocations());
        }

        /// <summary>
        /// Get count of Device Types using correct version of all <see cref="Location"/>
        /// </summary>
        /// <returns>List of <see cref="DetectionTypeGroup"/></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(List<DetectionTypeGroup>), Status200OK)]
        public IActionResult GetDetectionTypeCount([FromQuery] DateTime date)
        {
            var result = _repository.GetDetectionTypeCountForVersions(date);
            return Ok(result);
        }

        /// <summary>
        /// Gets an optimized list of <see cref="SearchLocation"/> to use for Location selection
        /// </summary>
        /// <param name="areaId">Locations by area</param>
        /// <param name="regionId">Locations by region</param>
        /// <param name="jurisdictionId">Locations by jurisdiction</param>
        /// <param name="metricTypeId">Locations by chart type</param>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<SearchLocation>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult GetLocationsForSearch([FromQuery] int? areaId, [FromQuery] int? regionId, [FromQuery] int? jurisdictionId, [FromQuery] int? metricTypeId)
        {
            var basicCharts = new List<int> { 1, 2, 3, 4, 14, 15, 17, 31 };
            var result = _repository.GetList()
                .FromSpecification(new ActiveLocationSpecification())
                .Where(w => (jurisdictionId != null) ? w.JurisdictionId == jurisdictionId : true)
                .Where(w => (regionId != null) ? w.RegionId == regionId : true)
                .Where(w => (areaId != null) ? w.Areas.Any(a => a.Id == areaId) : true)
                .Where(w => (metricTypeId != null) ? w.Approaches.Any(m => m.Detectors.Any(d => d.DetectionTypes.Any(t => t.MeasureTypes.Any(a => a.Id == metricTypeId)))) : true)
                .Select(s => new SearchLocation()
                {
                    Id = s.Id,
                    Start = s.Start,
                    locationIdentifier = s.LocationIdentifier,
                    PrimaryName = s.PrimaryName,
                    SecondaryName = s.SecondaryName,
                    RegionId = s.RegionId,
                    JurisdictionId = s.JurisdictionId,
                    Longitude = s.Longitude,
                    Latitude = s.Latitude,
                    ChartEnabled = s.ChartEnabled,
                    LocationTypeId = s.LocationTypeId,
                    Areas = s.Areas.Select(a => a.Id),
                    Charts = s.Approaches.SelectMany(m => m.Detectors.SelectMany(d => d.DetectionTypes.SelectMany(t => t.MeasureTypes.Select(i => i.Id)))).Distinct(),
                })
                .GroupBy(r => r.locationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            foreach (var location in result)
            {
                if (location != null && location.Charts != null)
                {
                    location.Charts = location.Charts.Concat(basicCharts).Order();
                    if (location.LocationTypeId == ((int)LocationTypes.RM))
                    {
                        location.Charts = location.Charts.Append(37).Order();
                    }
                }
            }

            return Ok(result);
        }

        #endregion
    }
}