#region license
// Copyright 2024 Utah Department of Transportation
// for DatabaseInstallerService - %Namespace%/Program.cs
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

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Configuration.Identity;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Extensions;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Load appsettings.json and override with command-line args
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddCommandLine(args);  // Command-line args will override appsettings.json
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging();
                services.AddAtspmDbContext(hostContext);
                // Add Identity services
                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<IdentityContext>()
                    .AddDefaultTokenProviders();

                // Bind the configuration section to CommandLineOptions (default to appsettings.json)
                services.Configure<CommandLineOptions>(hostContext.Configuration);

                // Add the hosted service
                services.AddHostedService<DatabaseInstallerService>();
            });

}



public class DatabaseInstallerService : IHostedService
{
    private readonly ILogger<DatabaseInstallerService> _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandLineOptions _commandLineOptions;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public DatabaseInstallerService(
        ILogger<DatabaseInstallerService> log,
        IServiceProvider serviceProvider,
        IOptions<CommandLineOptions> commandLineOptions,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _log = log;
        _serviceProvider = serviceProvider;
        _commandLineOptions = commandLineOptions.Value;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                if (_commandLineOptions.Migrate)
                {
                    // Apply migrations for each context using command-line options for connection strings
                    await ApplyMigrationsForAllContexts(scope.ServiceProvider, cancellationToken);
                }

                if (_commandLineOptions.SeedAdmin)
                {
                    // Seed admin user and role based on command-line options
                    await SeedAdminUserAndAssignRole(scope.ServiceProvider);
                }

                _log.LogInformation("Database migration and admin seeding completed successfully.");
            }
        }
        catch (Exception e)
        {
            _log.LogError("Exception during migration or seeding: {Exception}", e);
        }
        finally
        {
            _log.LogInformation("Shutting down the application after completion.");
            _hostApplicationLifetime.StopApplication();  // Stop the host after all tasks are complete
        }
    }

    private async Task ApplyMigrationsForAllContexts(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            // Use command-line arguments to set the connection strings dynamically
            var configContext = serviceProvider.GetRequiredService<ConfigContext>();
            configContext.Database.GetDbConnection().ConnectionString = _commandLineOptions.ConfigConnectionString;
            _log.LogInformation("Applying migrations for ConfigContext");
            await configContext.Database.MigrateAsync(cancellationToken);

            var aggregationContext = serviceProvider.GetRequiredService<AggregationContext>();
            aggregationContext.Database.GetDbConnection().ConnectionString = _commandLineOptions.AggregationConnectionString;
            _log.LogInformation("Applying migrations for AggregationContext");
            await aggregationContext.Database.MigrateAsync(cancellationToken);

            var eventLogContext = serviceProvider.GetRequiredService<EventLogContext>();
            eventLogContext.Database.GetDbConnection().ConnectionString = _commandLineOptions.EventLogConnectionString;
            _log.LogInformation("Applying migrations for EventLogContext");
            await eventLogContext.Database.MigrateAsync(cancellationToken);

            var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
            identityContext.Database.GetDbConnection().ConnectionString = _commandLineOptions.IdentityConnectionString;
            _log.LogInformation("Applying migrations for IdentityContext");
            await identityContext.Database.MigrateAsync(cancellationToken);
            await RolesAndClaimsDBInitializer.SeedRolesAndClaims(serviceProvider, _commandLineOptions.IdentityConnectionString);
        }
        catch (Exception ex)
        {
            _log.LogError("Error applying migrations: {Exception}", ex);
        }
    }

    private async Task SeedAdminUserAndAssignRole(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Use command-line options for admin user configuration
        var adminEmail = _commandLineOptions.AdminEmail;
        var adminPassword = _commandLineOptions.AdminPassword;
        var adminRole = _commandLineOptions.AdminRole;

        // Check if the admin user already exists
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            // Create the admin user
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                _log.LogInformation("Admin user created.");
            }
            else
            {
                _log.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        // Assign the Admin role to the admin user (role must already exist)
        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
            _log.LogInformation("Admin user assigned to Admin role.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));
        Console.WriteLine($"Operation Completed or Cancelled...");
        return Task.CompletedTask;
    }
}



// AdminUserOptions class to hold admin user data from appsettings.json
public class CommandLineOptions
{
    public bool Migrate { get; set; } = true;  // Option to trigger migrations
    public bool SeedAdmin { get; set; } = true; // Option to seed the admin user

    // New options to pass via the command line
    public string Provider { get; set; }  // e.g., "PostgreSQL"
    public string ConfigConnectionString { get; set; }
    public string AggregationConnectionString { get; set; }
    public string EventLogConnectionString { get; set; }
    public string IdentityConnectionString { get; set; }
    public string AdminEmail { get; set; }
    public string AdminPassword { get; set; }
    public string AdminRole { get; set; }
}

