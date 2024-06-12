#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Controllers/ExternalLinkController.cs
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
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// ExternalLinks Controller
    /// </summary>
    [ApiVersion(0.1)]
    public class ExternalLinkController : AtspmConfigControllerBase<ExternalLink, int>
    {
        private readonly IExternalLinksRepository _repository;

        /// <inheritdoc/>
        public ExternalLinkController(IExternalLinksRepository repository) : base(repository)
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
