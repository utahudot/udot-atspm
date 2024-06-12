#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Utility/CompressionTypeConverter.cs
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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ATSPM.Data.Utility
{
    /// <summary>
    /// <see cref="ValueConverter"/> used to convert a database string to an assembly type
    /// </summary>
    internal class CompressionTypeConverter : ValueConverter<Type, string>
    {
        /// <summary>
        /// <see cref="ValueConverter"/> used to convert a database string to an assembly type
        /// </summary>
        /// <param name="nameSpace">Namespace of class to convert to/from</param>
        /// <param name="assembly">Assembly of class to convert to/from</param>
        public CompressionTypeConverter(string nameSpace, string assembly) : base(v => v.Name, v => Type.GetType($"{nameSpace}.{v}, {assembly}")) { }
    }
}
