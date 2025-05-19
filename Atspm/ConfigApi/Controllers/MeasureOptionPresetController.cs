#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/MeasureOptionPresetController.cs
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
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Measure option presets Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MeasureOptionPresetController : LoggedInControllerBase<MeasureOptionPreset, int>
    {
        private readonly IMeasureOptionPresetRepository _repository;

        /// <inheritdoc/>
        public MeasureOptionPresetController(IMeasureOptionPresetRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        #endregion

        #region Actions

        #endregion

        #region Functions

        /// <summary>
        /// Retrieves a list of measure option preset types.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of measure option preset type names.</returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(List<string>), Status200OK)]
        public IActionResult GetMeasureOptionPresetTypes()
        {
            var result = typeof(AtspmOptionsBase).Assembly.GetTypes()
                .Where(w => w.IsSubclassOf(typeof(AtspmOptionsBase)))
                .Select(s => s.Name)
                .OrderBy(o => o)
                .ToList();

            return Ok(result);
        }

        #endregion
    }
}
