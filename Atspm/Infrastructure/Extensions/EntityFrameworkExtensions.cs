using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.
    /// </summary>
    public static class MigrationExtensions
    {
        /// <summary>
        /// Applies pending migrations for the specified <typeparamref name="TContext"/>.
        /// Ensures the physical database is created before migrations are applied.
        /// </summary>
        /// <typeparam name="TContext">The type of <see cref="DbContext"/> to migrate.</typeparam>
        /// <param name="host">The application host providing access to services.</param>
        /// <param name="seedAction">An optional asynchronous action to perform database seeding.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ApplyMigrations<TContext>(this IHost host, Func<IServiceProvider, Task>? seedAction = null) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var options = services.GetRequiredService<IOptionsSnapshot<DatabaseConfiguration>>();
            var settings = options.Get(typeof(TContext).Name);

            if (!settings.RunMigrations)
            {
                return;
            }

            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetRequiredService<TContext>();

            try
            {
                logger.LogInformation("Ensuring database for {ContextName} exists on {Host}...", typeof(TContext).Name, settings.Host);

                var databaseCreator = context.GetService<IRelationalDatabaseCreator>();

                if (!await databaseCreator.ExistsAsync())
                {
                    logger.LogInformation("Database for {ContextName} does not exist. Creating...", typeof(TContext).Name);
                    await databaseCreator.CreateAsync();
                }

                logger.LogInformation("Applying migrations for {ContextName}...", typeof(TContext).Name);
                await context.Database.MigrateAsync();

                if (seedAction != null)
                {
                    await seedAction(services);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating {ContextName}.", typeof(TContext).Name);
                throw;
            }
        }

        /// <summary>
        /// Seeds a default administrative user and role based on environment variables.
        /// </summary>
        /// <param name="services">The service provider used to resolve Identity services.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SeedAdminUser(this IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<IdentityContext>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                logger.LogWarning("Admin seeding skipped: Missing ADMIN_EMAIL or ADMIN_PASSWORD environment variables.");
                return;
            }

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                logger.LogInformation("Admin user {Email} not found. Creating...", email);

                var user = new ApplicationUser { UserName = email, Email = email };
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var adminRole = AtspmAuthorization.Roles.Admin;

                    if (!await roleManager.RoleExistsAsync(adminRole))
                    {
                        logger.LogInformation("Role {Role} not found during user seeding, creating it now.", adminRole);
                        await roleManager.CreateAsync(new IdentityRole(adminRole));
                    }

                    var addToRoleResult = await userManager.AddToRoleAsync(user, adminRole);

                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("Admin user {Email} created and assigned to {Role} successfully.", email, adminRole);
                    }
                    else
                    {
                        logger.LogError("User created but failed to assign to role {Role}: {Errors}",
                            adminRole, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogDebug("Admin user {Email} already exists. Skipping user creation.", email);
            }
        }

        /// <summary>
        /// Seeds the roles and granular claims required for ATSPM authorization.
        /// Only runs if the Admin role is missing.
        /// </summary>
        public static async Task SeedIdentityData(this IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = services.GetRequiredService<ILogger<IdentityContext>>();

            if (await roleManager.RoleExistsAsync(AtspmAuthorization.Roles.Admin))
            {
                logger.LogInformation("Identity roles already exist. Skipping claims seeding.");
                return;
            }

            foreach (var entry in AtspmAuthorization.RoleClaimsMap)
            {
                var role = await roleManager.FindByNameAsync(entry.Key);

                if (role == null)
                {
                    role = new IdentityRole(entry.Key);

                    var result = await roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        logger.LogInformation("Created role: {Role}", entry.Key);
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError("Could not create role {Role}: {Errors}", entry.Key, errors);
                        continue;
                    }
                }

                var existingClaims = await roleManager.GetClaimsAsync(role);
                foreach (var permission in entry.Value)
                {
                    if (!existingClaims.Any(c => c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(role, new Claim(AtspmAuthorization.RoleClaimType, permission));
                        logger.LogDebug("Added permission {Permission} to role {Role}", permission, entry.Key);
                    }
                }
            }
        }
    }
}
