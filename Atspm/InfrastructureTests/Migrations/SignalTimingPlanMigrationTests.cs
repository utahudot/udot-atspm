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

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Xunit;

namespace Utah.Udot.Atspm.InfrastructureTests.Migrations
{
    public class SignalTimingPlanMigrationTests
    {
        [Fact]
        public void SqlServerScriptsCreateAndUpgradeTheComputedValidityColumn()
        {
            const string creationMigration = "20260507203906_5_3";
            const string validityMigration = "20261124130215_FixSignalTimingPlanValidity";
            const string expression = "CAST(CASE WHEN [End] > [Start] THEN 1 ELSE 0 END AS bit)";
            var settings = new Dictionary<string, string>
            {
                ["DatabaseConfiguration:AggregationContext:DBType"] = "SqlServer",
                ["DatabaseConfiguration:AggregationContext:Host"] = "localhost",
                ["DatabaseConfiguration:AggregationContext:Database"] = "migration_test"
            };
            using var host = new HostBuilder()
                .ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings))
                .ConfigureServices((context, services) => services.AddAtspmDbContext(context))
                .Build();
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AggregationContext>();
            var migrator = context.GetService<IMigrator>();

            var creationScript = migrator.GenerateScript(toMigration: creationMigration);
            Assert.Contains("CREATE TABLE [SignalTimingPlans]", creationScript);
            Assert.Contains(expression, creationScript);

            var upgradeScript = migrator.GenerateScript(creationMigration, validityMigration);
            Assert.Contains("ALTER TABLE [SignalTimingPlans] DROP COLUMN [Valid]", upgradeScript);
            Assert.Contains(expression, upgradeScript);
        }
    }
}
