#region license
// Copyright 2026 Utah Department of Transportation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;

namespace Utah.Udot.Atspm.SqlDatabaseProvider
{
    /// <summary>
    /// Allows fresh installs to execute the original timing-plan migration.
    /// </summary>
    public sealed class SqlServerTimingPlanMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        ICommandBatchPreparer commandBatchPreparer)
        : SqlServerMigrationsSqlGenerator(dependencies, commandBatchPreparer)
    {
        /// <inheritdoc />
        protected override void ComputedColumnDefinition(
            string? schema, string table, string name, ColumnOperation operation,
            IModel? model, MigrationCommandListBuilder builder)
        {
            var originalSql = operation.ComputedColumnSql;
            if (table == "SignalTimingPlans" && name == "Valid" && originalSql == "[End] > [Start]")
            {
                // The historical migration supplied a predicate where SQL Server requires a scalar.
                operation.ComputedColumnSql = "CAST(CASE WHEN [End] > [Start] THEN 1 ELSE 0 END AS bit)";
            }

            try
            {
                base.ComputedColumnDefinition(schema, table, name, operation, model, builder);
            }
            finally
            {
                operation.ComputedColumnSql = originalSql;
            }
        }
    }
}
