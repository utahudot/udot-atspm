#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/ApproachController.cs
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
using Utah.Udot.Atspm.ConfigApi.Services;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.ConfigApi.DTO;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Approaches Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class ApproachController : LocationPolicyControllerBase<Approach, int>
    {
        private readonly IApproachRepository _repository;
        private readonly IApproachService approachService;

        /// <inheritdoc/>
        public ApproachController(IApproachRepository repository, IApproachService approachService) : base(repository)
        {
            _repository = repository;
            this.approachService = approachService;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="Detector"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Detector>> GetDetectors([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Detector>>(key);
        }

        #endregion

        #region Actions

        #endregion

        #region Functions

        [HttpPost("api/v1/UpsertApproach")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> UpsertApproach([FromBody] ApproachDto approach)
        {
            if (approach == null)
            {
                return BadRequest();
            }
            try
            {
                var approachResult = await approachService.UpsertApproachAsync(approach);
                return Ok(approachResult);
            }
            catch (Exception ex)
            {
                return StatusCode(Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("api/v1/GetApproachDto/{id}")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> GetApproachDto(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var approachResult = await approachService.GetApproachDtoByIdAsync(id);
                return Ok(approachResult);
            }
            catch (Exception ex)
            {
                return StatusCode(Status500InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
