#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Configuration/MeasureOptionPresetOdataConfiguration.cs
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
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.ConfigApi.Configuration
{
    /// <summary>
    /// Measure option presets oData configuration
    /// </summary>
    public class MeasureOptionPresetOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix)
        {
            var model = builder.EntitySet<MeasureOptionPreset>("MeasureOptionPreset").EntityType;
            model.Page(default, default);
            model.Expand(1, SelectExpandType.Automatic, new string[] { "measureType" });

            foreach (var opt in typeof(AtspmOptionsBase).Assembly.GetTypes().Where(w => w.IsSubclassOf(typeof(AtspmOptionsBase))))
            {
                builder.AddComplexType(opt).Namespace = nameof(AtspmOptionsBase);
            }

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Name).MaxLength = 512;

                        var a = model.Collection.Function("GetMeasureOptionPresetTypes");
                        a.ReturnsCollection<string>();

                        break;
                    }
            }
        }
    }
}
