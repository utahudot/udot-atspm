#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Utility/DateTimeOffsetToUtcConverter.cs
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
    /// Bridges non-nullable <see cref="DateTimeOffset"/> model properties to a <see cref="DateTime"/> store type,
    /// ensuring values are always normalized to UTC before being written to or read from the database.
    /// Using <see cref="DateTime"/> as the provider type allows compatibility with both SQL Server <c>datetime2</c>
    /// and PostgreSQL <c>timestamp</c> columns without requiring provider-specific workarounds.
    /// </summary>
    public class DateTimeOffsetToUtcConverter : ValueConverter<DateTimeOffset, DateTime>
    {
        /// <summary>
        /// Bridges non-nullable <see cref="DateTimeOffset"/> model properties to a <see cref="DateTime"/> store type,
        /// ensuring values are always normalized to UTC before being written to or read from the database.
        /// </summary>
        public DateTimeOffsetToUtcConverter()
            : base(
                csharp => csharp.ToUniversalTime().UtcDateTime,
                database => new DateTimeOffset(database, TimeSpan.Zero)
            )
        { }
    }
}
