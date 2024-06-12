#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Configuration/LocationOdataConfiguration.cs
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
using ATSPM.ConfigApi.Models;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;
using System.Net;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Location oData configuration
    /// </summary>
    public class LocationOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Location>("Location").EntityType;
            model.Page(default, default);
            model.Expand(1, SelectExpandType.Automatic, new string[] { "jurisdiction", "region" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Latitude).IsRequired();
                        model.Property(p => p.Longitude).IsRequired();
                        model.Property(p => p.Note).IsRequired();
                        model.Property(p => p.PrimaryName).IsRequired();
                        model.Property(p => p.LocationIdentifier).IsRequired();

                        model.Property(p => p.PrimaryName).MaxLength = 100;
                        model.Property(p => p.SecondaryName).MaxLength = 100;
                        model.Property(p => p.LocationIdentifier).MaxLength = 10;
                        model.Property(p => p.Note).MaxLength = 256;

                        model.Property(p => p.JurisdictionId).DefaultValueString = "0";
                        //model.EnumProperty(p => p.VersionAction).DefaultValueString = "10";
                        model.Property(p => p.Note).DefaultValueString = "Initial";

                        var a = model.Collection.Function("GetAllVersionsOfLocation");
                        a.Parameter<string>("identifier");
                        a.ReturnsCollectionFromEntitySet<Location>("Locations");

                        var b = model.Collection.Function("GetLatestVersionOfAllLocations");
                        b.ReturnsCollectionFromEntitySet<Location>("Locations");

                        var c = model.Collection.Function("GetLatestVersionOfLocation");
                        c.Parameter<string>("identifier");
                        c.ReturnsFromEntitySet<Location>("Location");

                        var d = model.Action("CopyLocationToNewVersion");
                        d.ReturnsFromEntitySet<Location>("Location");

                        var e = model.Action("SetLocationToDeleted");

                        var f = model.Collection.Function("GetLocationsForSearch");
                        //f.Parameter<int>("areaId");
                        //f.Parameter<int>("regionId");
                        //f.Parameter<int>("jurisdictionId");
                        //f.Parameter<int>("metricTypeId");
                        f.ReturnsCollectionFromEntitySet<SearchLocation>("SearchLocations");

                        break;
                    }
            }
        }
    }
}
