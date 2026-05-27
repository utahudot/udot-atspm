#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Utility/ConventionExtensions.cs
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

namespace Utah.Udot.Atspm.Data.Utility
{
    using Microsoft.EntityFrameworkCore;
    using System;

    /// <summary>
    /// Provides extension methods for <see cref="ModelConfigurationBuilder"/> to centrally configure 
    /// shared date and time conventions across multiple database providers.
    /// </summary>
    public static class ConventionExtensions
    {
        /// <summary>
        /// Applies global value converters to all <see cref="DateTimeOffset"/> and <see cref="Nullable{DateTimeOffset}"/> 
        /// properties to ensure absolute system timestamps are always normalized to UTC (+00:00).
        /// </summary>
        /// <param name="configurationBuilder">The model configuration builder being extended.</param>
        /// <returns>The updated <see cref="ModelConfigurationBuilder"/> instance for method chaining.</returns>
        public static ModelConfigurationBuilder ApplyDateTimeOffsetConverters(this ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToUtcConverter>();

            configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<NullableDateTimeOffsetToUtcConverter>();

            return configurationBuilder;
        }

        /// <summary>
        /// Configures the default database column types for <see cref="DateTime"/> properties based on the active database provider, 
        /// ensuring that local, device-specific timestamps are stored without timezone manipulation (as raw "wall-clock" digits).
        /// </summary>
        /// <param name="configurationBuilder">The model configuration builder being extended.</param>
        /// <param name="providerName">The name of the active Entity Framework Core database provider (e.g., <c>Database.ProviderName</c>).</param>
        /// <returns>The updated <see cref="ModelConfigurationBuilder"/> instance for method chaining.</returns>
        /// <remarks>
        /// This method maps <see cref="DateTime"/> to timezone-naive types like <c>timestamp</c> in PostgreSQL or <c>datetime2</c> in SQL Server. 
        /// Database providers that handle native types or text storage implicitly (such as SQLite or InMemory) are safely ignored.
        /// </remarks>
        public static ModelConfigurationBuilder ApplyProviderDateTimeTypes(this ModelConfigurationBuilder configurationBuilder, string? providerName)
        {
            var columnType = providerName switch
            {
                "Npgsql.EntityFrameworkCore.PostgreSQL" => "timestamp",
                "Microsoft.EntityFrameworkCore.SqlServer" => "datetime2",
                "Oracle.EntityFrameworkCore" => "TIMESTAMP",
                "Pomelo.EntityFrameworkCore.MySql" => "datetime",
                _ => null
            };

            if (columnType != null)
            {
                configurationBuilder.Properties<DateTime>().HaveColumnType(columnType);
            }

            return configurationBuilder;
        }
    }
}
