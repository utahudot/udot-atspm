#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Configuration/UsageEntryOdataConfiguration.cs
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
using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.Atspm.ConfigApi.Configuration
{
    /// <summary>
    /// UsageEntry oData configuration
    /// </summary>
    public class UsageEntryOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix)
        {
            var model = builder.EntitySet<UsageEntry>("UsageEntries")
                .EntityType
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        // Configure string property constraints similar to EF configuration
                        model.Property(p => p.ApiName).MaxLength = 32;
                        model.Property(p => p.TraceId).MaxLength = 100;
                        model.Property(p => p.ConnectionId).MaxLength = 100;
                        model.Property(p => p.RemoteIp).MaxLength = 45;
                        model.Property(p => p.UserAgent).MaxLength = 1024;
                        model.Property(p => p.UserId).MaxLength = 200;
                        model.Property(p => p.Route).MaxLength = 2000;
                        model.Property(p => p.QueryString).MaxLength = 2000;
                        model.Property(p => p.Method).MaxLength = 20;
                        model.Property(p => p.Controller).MaxLength = 200;
                        model.Property(p => p.Action).MaxLength = 200;
                        model.Property(p => p.ErrorMessage).MaxLength = 2000;

                        // Required properties
                        model.Property(p => p.Timestamp).IsRequired();
                        model.Property(p => p.StatusCode).IsRequired();
                        model.Property(p => p.DurationMs).IsRequired();
                        model.Property(p => p.Success).IsRequired();

                        // Nullable properties
                        model.Property(p => p.ResultCount).IsNullable();
                        model.Property(p => p.ResultSizeBytes).IsNullable();
                        model.Property(p => p.ErrorMessage).IsNullable();

                        break;
                    }
            }
        }
    }
}
