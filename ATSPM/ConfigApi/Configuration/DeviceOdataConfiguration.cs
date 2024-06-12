#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Configuration/DeviceOdataConfiguration.cs
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
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Net;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Device oData configuration
    /// </summary>
    public class DeviceOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Device>("Device").EntityType;
            model.Page(default, default);
            model.Expand(2, SelectExpandType.Automatic, new string[] { "location", "deviceConfiguration", "product" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Ipaddress).IsRequired();
                        model.Property(p => p.Ipaddress).MaxLength = 15;

                        model.Property(p => p.Notes).MaxLength = 512;

                        var c = model.Collection.Function("GetActiveDevicesByLocation");
                        c.Parameter<int>("locationId");
                        c.ReturnsFromEntitySet<Device>("Devices");

                        break;
                    }
            }
        }
    }
}