#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/DatabaseConfiguration.cs
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

using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.
    /// </summary>
    [ConfigurationSection(nameof(DatabaseConfiguration), null)]
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Gets or sets the type of database provider 
        /// </summary>
        [EnumDataType(typeof(DatabaseProvider), ErrorMessage = "Invalid database provider.")]
        public DatabaseProvider DBType { get; set; } = DatabaseProvider.InMemory;

        /// <summary>
        /// Gets or sets the network address or hostname of the database server.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Database Host is required.")]
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port number used to connect to the database server. 
        /// If null, a provider-specific default port is used.
        /// </summary>
        [Range(1, 65535, ErrorMessage = "Port must be a valid network port between 1 and 65535.")]
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the name of the specific database or the file path for file-based databases like SQLite.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Database Name is required.")]
        public string Database { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username for database authentication.
        /// </summary>
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for database authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether database migrations should be executed automatically on startup.
        /// </summary>
        public bool RunMigrations { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of additional provider-specific connection options.
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new();

        /// <summary>
        /// Constructs a valid connection string based on the <see cref="DBType"/> and provided credentials.
        /// </summary>
        /// <returns>A formatted connection string compatible with the chosen database provider.</returns>
        public string BuildConnectionString()
        {
            DbConnectionStringBuilder builder = DBType switch
            {
                DatabaseProvider.SqlServer => new SqlConnectionStringBuilder
                {
                    DataSource = Host,
                    InitialCatalog = Database,
                    UserID = User,
                    Password = Password
                },
                DatabaseProvider.PostgreSql => new NpgsqlConnectionStringBuilder
                {
                    Host = Host,
                    Port = Port ?? 5432,
                    Database = Database,
                    Username = User,
                    Password = Password
                },
                DatabaseProvider.MySql => new MySqlConnectionStringBuilder
                {
                    Server = Host,
                    Port = (uint)(Port ?? 3306),
                    Database = Database,
                    UserID = User,
                    Password = Password
                },
                DatabaseProvider.Oracle => new OracleConnectionStringBuilder
                {
                    DataSource = $"{Host}:{Port ?? 1521}/{Database}",
                    UserID = User,
                    Password = Password
                },
                DatabaseProvider.Sqlite => new SqliteConnectionStringBuilder
                {
                    DataSource = $"{Database}.db"
                },
                DatabaseProvider.InMemory or _ => new DbConnectionStringBuilder
                {
                    { "DataSource", string.IsNullOrEmpty(Database) ? Guid.NewGuid().ToString() : Database }
                }
            };

            string baseConnString = builder.ConnectionString;

            if (Options.Any())
            {
                var extraOptions = string.Join(";", Options.Select(x => $"{x.Key}={x.Value}"));
                return $"{baseConnString.TrimEnd(';')};{extraOptions}";
            }

            return baseConnString;
        }
    }
}
