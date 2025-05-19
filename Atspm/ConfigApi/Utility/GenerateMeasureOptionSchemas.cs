#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.Utility/GenerateMeasureOptionSchemas.cs
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

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.ATSPM.ConfigApi.Utility
{
    /// <summary>
    /// A document filter that generates schemas for all subclasses of <see cref="AtspmOptionsBase"/>.
    /// </summary>
    public class GenerateMeasureOptionSchemas : IDocumentFilter
    {
        /// <summary>
        /// Applies the document filter to generate schemas for all subclasses of <see cref="AtspmOptionsBase"/>.
        /// </summary>
        /// <param name="swaggerDoc">The OpenAPI document.</param>
        /// <param name="context">The document filter context.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var i in typeof(AtspmOptionsBase).Assembly.GetTypes().Where(w => w.IsSubclassOf(typeof(AtspmOptionsBase))))
            {
                context.SchemaGenerator.GenerateSchema(i, context.SchemaRepository);
            }
        }
    }
}
