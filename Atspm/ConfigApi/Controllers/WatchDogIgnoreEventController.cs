#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/WatchDogIgnoreEventController.cs
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
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// WatchDogIgnoreEvent Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class WatchDogIgnoreEventController : WatchdogPolicyControllerBase<WatchDogIgnoreEvent, int>
    {
        private readonly IWatchDogIgnoreEventRepository _repository;

        /// <inheritdoc/>
        public WatchDogIgnoreEventController(IWatchDogIgnoreEventRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        #endregion

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}
