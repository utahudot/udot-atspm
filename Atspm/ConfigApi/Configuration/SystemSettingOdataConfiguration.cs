#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Configuration/SystemSettingOdataConfiguration.cs
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
using Asp.Versioning.OData;
using Microsoft.OData.ModelBuilder;
using Utah.Udot.ATSPM.ConfigApi.DTO;

namespace Utah.Udot.Atspm.ConfigApi.Configuration
{
    /// <summary>
    /// SystemSettings OData configuration.
    /// </summary>
    public class SystemSettingOdataConfiguration : IModelConfiguration
    {
        /// <inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix)
        {
            builder.ComplexType<UpdateSystemSettingDto>();

            var entityType = builder.EntitySet<SystemSettingDto>("SystemSettings").EntityType;
            entityType.HasKey(p => p.Key);
            entityType
                .Page(default, default)
                .Count()
                .Select()
                .OrderBy()
                .Filter();
        }
    }
}
