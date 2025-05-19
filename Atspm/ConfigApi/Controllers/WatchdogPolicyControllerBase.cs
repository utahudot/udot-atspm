#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/WatchdogPolicyControllerBase.cs
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Base class for controllers using watchdog policies
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <inheritdoc/>
    public class WatchdogPolicyControllerBase<T, TKey>(IAsyncRepository<T> repository) : ConfigControllerBase<T, TKey>(repository) where T : AtspmConfigModelBase<TKey>
    {
        /// <inheritdoc/>
        [Authorize(Policy = "CanViewWatchDog")]
        public override ActionResult<IQueryable<T>> Get(ODataQueryOptions<T> options)
        {
            return base.Get(options);
        }

        /// <inheritdoc/>
        [Authorize(Policy = "CanViewWatchDog")]
        public override ActionResult<T> Get(TKey key, ODataQueryOptions<T> options)
        {
            return base.Get(key, options);
        }

        /// <inheritdoc/>
        [Authorize(Policy = "CanViewWatchDog")]
        protected override ActionResult<TType> GetNavigationProperty<TType>(TKey key)
        {
            return base.GetNavigationProperty<TType>(key);
        }

        /// <inheritdoc/>
        [Authorize(Policy = "CanEditGeneralConfigurations")]
        public override Task<IActionResult> Post([FromBody] T item)
        {
            return base.Post(item);
        }

        /// <inheritdoc/>
        [Authorize(Policy = "CanEditGeneralConfigurations")]
        public override Task<IActionResult> Put(TKey key, [FromBody] T item)
        {
            return base.Put(key, item);
        }

        /// <inheritdoc/>
        [Authorize(Policy = "CanEditGeneralConfigurations")]
        public override Task<IActionResult> Patch(TKey key, [FromBody] Delta<T> item)
        {
            return base.Patch(key, item);
        }

        /// <inheritdoc/>
        [Authorize(Policy = "CanDeleteGeneralConfigurations")]
        public override Task<IActionResult> Delete(TKey key)
        {
            return base.Delete(key);
        }
    }
}

