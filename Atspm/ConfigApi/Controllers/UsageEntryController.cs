#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/UsageEntryController.cs
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
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Attributes;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Usage entry Controller
    /// </summary>
    /// <inheritdoc/>
    [ApiVersion(1.0)]
    public class UsageEntryController(IUsageEntryRepository repository) : ConfigControllerBase<UsageEntry, int>(repository)
    {
        private readonly IUsageEntryRepository _repository = repository;

        /// <inheritdoc/>
        [AuthorizePermission(AtspmAuthorization.Permissions.UsageView)]
        public override ActionResult<IQueryable<UsageEntry>> Get(ODataQueryOptions<UsageEntry> options)
        {
            return base.Get(options);
        }

        /// <inheritdoc/>
        [AuthorizePermission(AtspmAuthorization.Permissions.UsageView)]
        public override ActionResult<UsageEntry> Get(int key, ODataQueryOptions<UsageEntry> options)
        {
            return base.Get(key, options);
        }

        /// <inheritdoc/>
        [AuthorizePermission(AtspmAuthorization.Permissions.UsageView)]
        protected override ActionResult<TType> GetNavigationProperty<TType>(int key)
        {
            return base.GetNavigationProperty<TType>(key);
        }

        /// <inheritdoc/>
        [AuthorizePermission(AtspmAuthorization.Permissions.UsageEdit)]
        public override Task<IActionResult> Post([FromBody] UsageEntry item)
        {
            return base.Post(item);
        }

        /// <inheritdoc/>
        [AuthorizePermission(AtspmAuthorization.Permissions.UsageEdit)]
        public override Task<IActionResult> Put(int key, [FromBody] UsageEntry item)
        {
            return base.Put(key, item);
        }

        /// <inheritdoc/>
        [AuthorizePermission(AtspmAuthorization.Permissions.UsageEdit)]
        public override Task<IActionResult> Patch(int key, [FromBody] Delta<UsageEntry> item)
        {
            return base.Patch(key, item);
        }

        /// <inheritdoc/>
        [AuthorizePermission(AtspmAuthorization.Permissions.UsageDelete)]
        public override Task<IActionResult> Delete(int key)
        {
            return base.Delete(key);
        }

        #region NavigationProperties

        #endregion

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}
