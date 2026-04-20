#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/MigrationLogMessages.cs
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

using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Provides strongly-typed logging methods for database migration and initialization processes.
    /// </summary>
    /// <remarks>
    /// This class uses the .NET Source Generator for logging to provide high-performance 
    /// structured logging with pre-defined Event IDs (200-205).
    /// </remarks>
    public partial class MigrationLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationLogMessages"/> class.
        /// </summary>
        /// <param name="logger">The underlying logger instance.</param>
        /// <param name="contextName">The name of the DbContext to be included as a persistent 'context' label in all logs.</param>
        public MigrationLogMessages(ILogger logger, string contextName)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
        {
            { "context", contextName }
        });
        }

        /// <summary>
        /// Logs the status of the "Run Migrations" configuration flag.
        /// </summary>
        /// <param name="contextName">The name of the database context.</param>
        /// <param name="runMigrations">The value of the configuration flag.</param>
        [LoggerMessage(EventId = 200, EventName = "Run Migrations Flag", Level = LogLevel.Debug, Message = "Migrations configured to run on {contextName} - {runMigrations}")]
        public partial void RunMigrationsFlag(string contextName, bool runMigrations);

        /// <summary>
        /// Logs whether the target database currently exists before migration attempts.
        /// </summary>
        /// <param name="contextName">The name of the database context.</param>
        /// <param name="databaseExists">True if the database is detected, false otherwise.</param>
        [LoggerMessage(EventId = 201, EventName = "Ensuring Database Exists", Level = LogLevel.Debug, Message = "Ensure database exists for {contextName} - {databaseExists}")]
        public partial void DatabaseExists(string contextName, bool databaseExists);

        /// <summary>
        /// Logs that a new database creation process has been initiated.
        /// </summary>
        /// <param name="contextName">The name of the database context.</param>
        [LoggerMessage(EventId = 202, EventName = "Create Database", Level = LogLevel.Information, Message = "Creating database for {contextName}")]
        public partial void CreateDatabase(string contextName);

        /// <summary>
        /// Logs that pending migrations are being applied to the database.
        /// </summary>
        /// <param name="contextName">The name of the database context.</param>
        [LoggerMessage(EventId = 203, EventName = "Applying Migrations", Level = LogLevel.Information, Message = "Applying migrations for {contextName}")]
        public partial void ApplyngMigrations(string contextName);

        /// <summary>
        /// Logs that data seeding actions are being executed after a successful migration.
        /// </summary>
        /// <param name="contextName">The name of the database context.</param>
        [LoggerMessage(EventId = 204, EventName = "Apply Seeding", Level = LogLevel.Information, Message = "Applying seeding actions for {contextName}")]
        public partial void ApplySeeding(string contextName);

        /// <summary>
        /// Logs a critical failure during the migration or seeding process.
        /// </summary>
        /// <param name="contextName">The name of the database context where the failure occurred.</param>
        /// <param name="ex">The exception that triggered the failure.</param>
        [LoggerMessage(EventId = 205, EventName = "Apply Migrations Exception", Level = LogLevel.Error, Message = "Exception applying migrations for {contextName}")]
        public partial void ApplyMigrationsException(string contextName, Exception ex = null);
    }
}
