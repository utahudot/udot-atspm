#region license
// Copyright 2026 Utah Departement of Transportation
// for DataApi - Utah.Udot.ATSPM.DataApi.Dtos/PropertyMeta.cs
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

namespace Utah.Udot.ATSPM.DataApi.Dtos
{
    /// <summary>
    /// Represents metadata describing a single property within a data type.
    /// Used to expose property names and their associated XML documentation
    /// when generating API metadata.
    /// </summary>
    public record PropertyMeta
    {
        /// <summary>
        /// The name of the property being described.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The XML documentation summary associated with the property, if available.
        /// </summary>
        public string? Description { get; init; }
    }

    /// <summary>
    /// Represents metadata describing a data type, including its own documentation
    /// and the metadata for each of its public properties.
    /// </summary>
    public record DataTypeMeta : PropertyMeta
    {
        /// <summary>
        /// The collection of properties defined on the data type, each including
        /// its name and associated XML documentation summary.
        /// </summary>
        public IReadOnlyList<PropertyMeta> Properties { get; init; } = [];
    }

}
