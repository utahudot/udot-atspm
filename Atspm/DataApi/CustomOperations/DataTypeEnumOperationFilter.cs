#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.CustomOperations/DataTypeEnumOperationFilter.cs
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
using Utah.Udot.ATSPM.DataApi.Controllers;

namespace Utah.Udot.Atspm.DataApi.CustomOperations
{
    /// <summary>
    /// An <see cref="IOperationFilter"/> implementation that enriches Swagger/OpenAPI
    /// documentation for endpoints using a <c>dataType</c> parameter.
    /// </summary>
    /// <remarks>
    /// This filter inspects the declaring controller type, walks up the inheritance chain
    /// to locate the generic <c>DataControllerBase&lt;T1,T2&gt;</c>, and then uses the second
    /// generic argument (<c>T2</c>) to determine all valid derived types.
    /// These derived type names are registered as the allowed values for the <c>dataType</c>
    /// parameter in Swagger UI, enabling dropdown selection and discoverability.
    /// </remarks>
    public class DataTypeEnumOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the given OpenAPI operation.
        /// </summary>
        /// <param name="operation">
        /// The OpenAPI operation being processed. Contains metadata about parameters,
        /// responses, and other aspects of the endpoint.
        /// </param>
        /// <param name="context">
        /// Provides contextual information about the current operation, including
        /// the method and declaring type.
        /// </param>
        /// <remarks>
        /// This method:
        /// <list type="number">
        /// <item>Finds parameters named <c>dataType</c>.</item>
        /// <item>Walks up the declaring type’s inheritance chain until it finds
        /// <c>DataControllerBase&lt;T1,T2&gt;</c>.</item>
        /// <item>Extracts the second generic argument (<c>T2</c>), which represents
        /// the model base type for the controller.</item>
        /// <item>Calls <c>ListDerivedTypes()</c> on <c>T2</c> to get all valid derived type names.</item>
        /// <item>Registers those names as the <c>Enum</c> values for the parameter schema,
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var parameter in operation.Parameters.Where(p => p.Name == "dataType"))
            {
                var declaringType = context.MethodInfo.DeclaringType;
                var baseType = declaringType;

                // Walk up the inheritance chain until we find DataControllerBase<,>
                while (baseType != null && (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(DataControllerBase<,>)))
                {
                    baseType = baseType.BaseType;
                }

                if (baseType != null)
                {
                    var genericArgs = baseType.GetGenericArguments();
                    var t1 = genericArgs[0]; // first generic argument
                    var t2 = genericArgs[1]; // second generic argument

                    List<string> derivedTypes = t2.ListDerivedTypes().ToList();

                    // now you can assign to parameter.Schema.Enum
                    parameter.Schema.Enum = derivedTypes
                        .Select(name => new OpenApiString(name))
                        .Cast<IOpenApiAny>()
                        .ToList();
                }
            }
        }
    }
}
