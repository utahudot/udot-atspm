#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.CustomOperations/GenerateAggregationSchemas.cs
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

namespace Utah.Udot.Atspm.DataApi.CustomOperations
{
    /// <summary>
    /// A Swagger <see cref="IDocumentFilter"/> that ensures all aggregation model
    /// subclasses are included in the generated OpenAPI schema.
    /// </summary>
    /// <remarks>
    /// By default, Swashbuckle only generates schemas for types that are directly
    /// referenced in controller actions. This filter scans the assembly containing
    /// <see cref="AggregationModelBase"/> and registers schemas for all derived
    /// types so they appear in the OpenAPI document. This is useful when you want
    /// consumers to see all possible aggregation models even if they are not
    /// directly returned by an endpoint.
    /// </remarks>
    public class GenerateAggregationSchemas : IDocumentFilter
    {
        /// <summary>
        /// Applies the filter to the OpenAPI document by generating schemas for all
        /// subclasses of <see cref="AggregationModelBase"/>.
        /// </summary>
        /// <param name="swaggerDoc">
        /// The OpenAPI document being built. Not directly modified in this filter,
        /// but required by the interface.
        /// </param>
        /// <param name="context">
        /// Provides access to the schema generator and repository used to add new
        /// type definitions to the document.
        /// </param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var i in typeof(AggregationModelBase).Assembly
                         .GetTypes()
                         .Where(w => w.IsSubclassOf(typeof(AggregationModelBase))))
            {
                context.SchemaGenerator.GenerateSchema(i, context.SchemaRepository);
            }
        }
    }
}
