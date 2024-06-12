#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Configuration/VersionHistoryOdataConfiguration.cs
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
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Version history oData configuration
    /// </summary>
    public class VersionHistoryOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<VersionHistory>("VersionHistory")
                .EntityType
                .Page(default, default)
                .Expand(2, SelectExpandType.Automatic, new string[] { "children" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Name).IsRequired();
                        model.Property(p => p.Date).IsRequired();
                        model.Property(p => p.Version).IsRequired();

                        model.Property(p => p.Name).MaxLength = 64;
                        model.Property(p => p.Notes).MaxLength = 512;

                        break;
                    }
            }
        }
    }
}
