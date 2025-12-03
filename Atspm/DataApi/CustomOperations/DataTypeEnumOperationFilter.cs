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

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.Atspm.DataApi.CustomOperations
{
    public class DataTypeEnumOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Only apply to parameters named "dataType"
            foreach (var parameter in operation.Parameters.Where(p => p.Name == "dataType"))
            {
                // Determine which controller/action this belongs to
                var declaringType = context.MethodInfo.DeclaringType;

                List<string> derivedTypes = new();

                if (declaringType != null)
                {
                    if (declaringType.Name.Contains("AggregationController"))
                    {
                        derivedTypes = typeof(AggregationModelBase).ListDerivedTypes().ToList();
                    }
                    else if (declaringType.Name.Contains("EventLogController"))
                    {
                        derivedTypes = typeof(EventLogModelBase).ListDerivedTypes().ToList();
                    }
                }

                if (derivedTypes.Any())
                {
                    parameter.Schema.Enum = derivedTypes
                        .Select(name => new OpenApiString(name))
                        .Cast<IOpenApiAny>()
                        .ToList();
                }
            }
        }
    }
}
