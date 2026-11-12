#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/SystemSettingsController.cs
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
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Globalization;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.ConfigApi.DTO;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// OData controller for viewing and overriding system-configurable application settings.
    /// </summary>
    [ApiVersion(1.0)]
    [Authorize(Roles = AtspmAuthorization.Roles.Admin)]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class SystemSettingsController(
        IConfiguration configuration,
        ISystemSettingRepository repository,
        IReadOnlyList<SystemSettingDescriptor> descriptors) : ODataController
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ISystemSettingRepository _repository = repository;
        private readonly IReadOnlyList<SystemSettingDescriptor> _descriptors = descriptors;

        /// <summary>
        /// Retrieves all system-configurable settings with their effective values, XML descriptions, and default status.
        /// </summary>
        /// <param name="options">OData query options.</param>
        /// <returns>Queryable collection of <see cref="SystemSettingDto"/>.</returns>
        /// <response code="200">Items successfully retrieved.</response>
        [EnableQuery]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(typeof(IQueryable<SystemSettingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IQueryable<SystemSettingDto>> Get(ODataQueryOptions<SystemSettingDto> options)
        {
            var dbSettings = _repository.GetList()
                .ToDictionary(s => s.Key, s => (string?)s.Value, StringComparer.OrdinalIgnoreCase);

            var result = _descriptors.Select(d => MapToDto(d, dbSettings)).AsQueryable();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific system-configurable setting by its hierarchical configuration key.
        /// </summary>
        /// <param name="key">Hierarchical configuration key.</param>
        /// <param name="options">OData query options.</param>
        /// <returns>Matching <see cref="SystemSettingDto"/>.</returns>
        /// <response code="200">Item was successfully retrieved.</response>
        /// <response code="404">Item does not exist.</response>
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(typeof(SystemSettingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<SystemSettingDto> Get(string key, ODataQueryOptions<SystemSettingDto> options)
        {
            var descriptor = _descriptors.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));
            if (descriptor is null)
            {
                return NotFound(key);
            }

            var dbSetting = _repository.GetList(s => s.Key == descriptor.Key).FirstOrDefault();
            var dbMap = dbSetting is not null
                ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { [dbSetting.Key] = dbSetting.Value }
                : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            var dto = MapToDto(descriptor, dbMap);
            return Ok(SingleResult.Create(new[] { dto }.AsQueryable()).Queryable);
        }

        /// <summary>
        /// Creates or updates a database override value for the specified system-configurable setting key.
        /// </summary>
        /// <param name="key">Hierarchical configuration key to override.</param>
        /// <param name="item">Payload containing the new value to persist as an override.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">Override was successfully created or updated.</response>
        /// <response code="400">Value cannot be converted to the setting's target data type.</response>
        /// <response code="404">Key is not a valid system-configurable setting.</response>
        [HttpPut("api/v{version:apiVersion}/SystemSettings/{key}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromRoute] string key, [FromBody] UpdateSystemSettingDto item)
        {
            if (item is null)
            {
                return BadRequest("Request body is required.");
            }

            var descriptor = _descriptors.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));
            if (descriptor is null)
            {
                return NotFound($"Key '{key}' is not a valid system-configurable setting.");
            }

            if (item.Value is not null)
            {
                try
                {
                    var targetType = Nullable.GetUnderlyingType(descriptor.PropertyInfo.PropertyType)
                        ?? descriptor.PropertyInfo.PropertyType;
                    Convert.ChangeType(item.Value, targetType, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    return BadRequest($"Value '{item.Value}' is not valid for type '{descriptor.ValueType}'.");
                }
            }

            var existing = _repository.GetList(s => s.Key == descriptor.Key).FirstOrDefault();
            if (existing is null)
            {
                await _repository.AddAsync(new SystemSetting
                {
                    Key = descriptor.Key,
                    Value = item.Value
                });
            }
            else
            {
                existing.Value = item.Value;
                await _repository.UpdateAsync(existing);
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes the database override for the specified setting key, reverting it back to its external default value.
        /// </summary>
        /// <param name="key">Hierarchical configuration key to reset to default.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">Override successfully removed.</response>
        /// <response code="404">No override exists for the specified key.</response>
        [HttpDelete("api/v{version:apiVersion}/SystemSettings/{key}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] string key)
        {
            var descriptor = _descriptors.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));
            var lookupKey = descriptor?.Key ?? key;

            var existing = _repository.GetList(s => s.Key == lookupKey).FirstOrDefault();
            if (existing is null)
            {
                return NotFound();
            }

            await _repository.RemoveAsync(existing);

            return NoContent();
        }

        private SystemSettingDto MapToDto(SystemSettingDescriptor descriptor, IReadOnlyDictionary<string, string?> dbSettings)
        {
            var hasOverride = dbSettings.TryGetValue(descriptor.Key, out var dbValue);
            var externalValue = _configuration[descriptor.Key] ?? descriptor.CodeDefaultValue;

            return new SystemSettingDto
            {
                Key = descriptor.Key,
                Section = descriptor.Section,
                Name = descriptor.Name,
                Value = hasOverride ? dbValue : externalValue,
                Description = descriptor.Description,
                ValueType = descriptor.ValueType,
                IsDefault = !hasOverride
            };
        }
    }
}
