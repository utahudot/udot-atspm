#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.CustomOperations/TimestampFormatHeader.cs
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
    /// An <see cref="IOperationFilter"/> that adds a custom header parameter
    /// (<c>X-Timestamp-Format</c>) to all API operations in the OpenAPI specification.
    /// </summary>
    /// <remarks>
    /// This header allows clients to specify a custom timestamp format for CSV or other
    /// formatted responses. The format string must follow the .NET custom date and time
    /// format conventions:
    /// https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
    /// </remarks>
    public class TimestampFormatHeader : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the given <see cref="OpenApiOperation"/> by adding
        /// the <c>X-Timestamp-Format</c> header parameter definition.
        /// </summary>
        /// <param name="operation">
        /// The OpenAPI operation to which the header parameter will be added.
        /// </param>
        /// <param name="context">
        /// The filter context that provides metadata about the API operation.
        /// </param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Timestamp-Format",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Change CSV timestamp format. See: https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings",
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}