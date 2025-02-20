#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/UpdateCommandHostedService.cs
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

using global::DatabaseInstaller.Commands;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Configuration.Identity;
using Utah.Udot.Atspm.Data.Models;


namespace DatabaseInstaller.Services
{
    public class UpdateCommandHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UpdateCommandConfiguration _config;
        private readonly ILogger<UpdateCommandHostedService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public UpdateCommandHostedService(
            IServiceProvider serviceProvider,
            IOptions<UpdateCommandConfiguration> config,
            ILogger<UpdateCommandHostedService> logger,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Apply migrations
                await ApplyMigrationsForAllContexts(cancellationToken);

                // Optionally seed admin
                if (_config.SeedAdmin)
                {
                    await SeedAdminUserAndAssignRole();
                }

                _logger.LogInformation("Database migration and admin seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception during migration or seeding: {Exception}", ex);
            }
            finally
            {
                _logger.LogInformation("Shutting down the application after completion.");
                _hostApplicationLifetime.StopApplication(); // Stop the host after all tasks are complete
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task ApplyMigrationsForAllContexts(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var serviceProvider = scope.ServiceProvider;

                var configContext = serviceProvider.GetRequiredService<ConfigContext>();
                configContext.Database.GetDbConnection().ConnectionString = _config.ConfigConnection;
                _logger.LogInformation("Applying migrations for ConfigContext");
                await configContext.Database.MigrateAsync(cancellationToken);

                var aggregationContext = serviceProvider.GetRequiredService<AggregationContext>();
                aggregationContext.Database.GetDbConnection().ConnectionString = _config.AggregationConnection;
                _logger.LogInformation("Applying migrations for AggregationContext");
                await aggregationContext.Database.MigrateAsync(cancellationToken);

                var eventLogContext = serviceProvider.GetRequiredService<EventLogContext>();
                eventLogContext.Database.GetDbConnection().ConnectionString = _config.EventLogConnection;
                _logger.LogInformation("Applying migrations for EventLogContext");
                await eventLogContext.Database.MigrateAsync(cancellationToken);

                var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
                identityContext.Database.GetDbConnection().ConnectionString = _config.IdentityConnection;
                _logger.LogInformation("Applying migrations for IdentityContext");
                await identityContext.Database.MigrateAsync(cancellationToken);

                await RolesAndClaimsDBInitializer.SeedRolesAndClaims(serviceProvider, _config.IdentityConnection);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error applying migrations: {Exception}", ex);
            }
        }

        private async Task SeedAdminUserAndAssignRole()
        {
            using var scope = _serviceProvider.CreateScope();
            var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
            // Set the connection string dynamically
            identityContext.Database.GetDbConnection().ConnectionString = _config.IdentityConnection;
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Check if the admin user already exists
            var adminUser = await userManager.FindByEmailAsync(_config.AdminEmail);
            if (adminUser == null)
            {
                // Create the admin user
                adminUser = new ApplicationUser
                {
                    UserName = _config.AdminEmail,
                    Email = _config.AdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Admin",
                    Agency = "Transportation Agency",
                };
                var result = await userManager.CreateAsync(adminUser, _config.AdminPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin user created.");
                }
                else
                {
                    _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return;
                }
            }

            // Assign the Admin role to the admin user (role must already exist)
            if (!await userManager.IsInRoleAsync(adminUser, _config.AdminRole))
            {
                await userManager.AddToRoleAsync(adminUser, _config.AdminRole);
                _logger.LogInformation("Admin user assigned to Admin role.");
            }
        }
    }


}
