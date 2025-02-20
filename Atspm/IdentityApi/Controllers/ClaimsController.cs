#region license
// Copyright 2025 Utah Departement of Transportation
// for IdentityApi - Identity.Controllers/ClaimsController.cs
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
using Identity.Business.Claims;
using Identity.Models.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Enums;
using Utah.Udot.ATSPM.IdentityApi.Controllers;

namespace Identity.Controllers
{
    [Authorize()]
    [ApiVersion("1.0")]
    public class ClaimsController : IdentityControllerBase
    {
        private readonly ClaimsService claimsService;

        public ClaimsController(ClaimsService claimsService)
        {
            this.claimsService = claimsService;
        }

        // GET: api/claims
        [HttpGet]
        [Authorize(Policy = "CanViewRoles")]
        public async Task<IActionResult> GetClaims()
        {
            var descriptions = Enum.GetValues(typeof(ClaimTypes))
                                   .Cast<Enum>()
                                   .Select(e => e.GetDisplayName())
                                   .ToList();

            return Ok(descriptions);
        }

        // GET: api/claims/roleName
        [HttpGet("{roleName}")]
        [Authorize(Policy = "CanViewRoles")]
        public async Task<IActionResult> GetClaimsForRole(string roleName)
        {
            return Ok(await claimsService.GetAllClaimsForRole(roleName));
        }

        // POST: api/claims/roleName
        [HttpPost("{roleName}")]
        [Authorize(Policy = "CanEditRoles")]
        public async Task<IActionResult> AddClaimToRole(string roleName, [FromBody] ClaimModel claim)
        {
            if (await claimsService.AddClaimToRole(roleName, claim.Type, claim.Value))
                return Ok();
            return BadRequest("Could not add claim to role.");
        }

        // POST: api/claims/addClaims/roleName
        [HttpPost("add/{roleName}")]
        [Authorize(Policy = "CanEditRoles")]
        public async Task<IActionResult> AddClaimsToRole(string roleName, [FromBody] ClaimsModel model)
        {
            try
            {
                await claimsService.AddClaimsToRole(roleName, model.Claims);
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // DELETE: api/claims/roleName
        [HttpDelete("{roleName}")]
        [Authorize(Policy = "CanEditRoles")]
        public async Task<IActionResult> RemoveClaimFromRole(string roleName, [FromBody] ClaimModel claim)
        {
            if (await claimsService.RemoveClaimFromRole(roleName, claim.Type, claim.Value))
                return Ok();
            return BadRequest("Could not remove claim from role.");
        }
    }
}