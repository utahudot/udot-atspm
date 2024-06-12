#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Configuration/DeviceConfigurationOdataConfiguration.cs
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
    /// Device configuration oData configuration
    /// </summary>
    public class DeviceConfigurationOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<DeviceConfiguration>("DeviceConfiguration").EntityType;
            model.Page(default, default);
            model.Expand(1, SelectExpandType.Automatic, new string[] { "product" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Firmware).IsRequired();

                        model.Property(p => p.Firmware).MaxLength = 16;
                        model.Property(p => p.Notes).MaxLength = 512;
                        model.Property(p => p.Directory).MaxLength = 512;
                        model.Property(p => p.ConnectionTimeout).DefaultValueString = "2000";
                        model.Property(p => p.OperationTimout).DefaultValueString = "2000";
                        //model.Property(p => p.DataModel).MaxLength = 512;
                        model.Property(p => p.UserName).MaxLength = 50;
                        model.Property(p => p.Password).MaxLength = 50;

                        break;
                    }
            }
        }
    }
}
