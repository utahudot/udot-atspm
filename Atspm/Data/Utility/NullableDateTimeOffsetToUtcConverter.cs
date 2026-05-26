#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Utility/NullableDateTimeOffsetToUtcConverter.cs
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

namespace Utah.Udot.Atspm.Data.Utility
{
    /// <summary>
    /// Forces all nullable DateTimeOffset properties to UTC (+00:00) 
    /// while safely handling null database values.
    /// </summary>
    public class NullableDateTimeOffsetToUtcConverter : ValueConverter<DateTimeOffset?, DateTimeOffset?>
    {
        /// <summary>
        /// Forces all nullable DateTimeOffset properties to UTC (+00:00) 
        /// while safely handling null database values.
        /// </summary>
        public NullableDateTimeOffsetToUtcConverter()
            : base(
                csharp => csharp.HasValue ? csharp.Value.ToUniversalTime() : csharp,
                database => database.HasValue ? database.Value.ToUniversalTime() : database
            )
        { }
    }
}
