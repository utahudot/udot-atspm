using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.
    /// </summary>
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Gets or sets the type of database provider (e.g., "sqlserver", "postgresql", "mysql", "oracle", "sqlite").
        /// </summary>
        public string DBType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the network address or hostname of the database server.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port number used to connect to the database server. 
        /// If null, a provider-specific default port is used.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the name of the specific database or the file path for file-based databases like SQLite.
        /// </summary>
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
            DbConnectionStringBuilder builder = DBType.ToLower() switch
            {
                "sqlserver" => new SqlConnectionStringBuilder
                {
                    DataSource = Host,
                    InitialCatalog = Database,
                    UserID = User,
                    Password = Password
                },
                "postgresql" => new NpgsqlConnectionStringBuilder
                {
                    Host = Host,
                    Port = Port ?? 5432,
                    Database = Database,
                    Username = User,
                    Password = Password
                },
                "mysql" => new MySqlConnectionStringBuilder
                {
                    Server = Host,
                    Port = (uint)(Port ?? 3306),
                    Database = Database,
                    UserID = User,
                    Password = Password
                },
                "oracle" => new OracleConnectionStringBuilder
                {
                    DataSource = $"{Host}:{Port ?? 1521}/{Database}",
                    UserID = User,
                    Password = Password
                },
                "sqlite" => new SqliteConnectionStringBuilder
                {
                    DataSource = $"{Database}.db"
                },
                _ => new DbConnectionStringBuilder
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
