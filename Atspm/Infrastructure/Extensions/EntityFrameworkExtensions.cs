using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data.Models.IdentityModels;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrations<TContext>(this IHost host, bool seedAdmin = false) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            bool autoMigrate = configuration.GetValue<bool>("DatabaseOptions:AutoMigrate");

            if (!autoMigrate)
            {
                logger.LogInformation("Auto-migration is disabled for {ContextName}.", typeof(TContext).Name);
                return;
            }

            try
            {
                var context = services.GetRequiredService<TContext>();

                logger.LogInformation("Applying migrations for {ContextName}...", typeof(TContext).Name);
                await context.Database.MigrateAsync();

                if (seedAdmin)
                {
                    await SeedAdminUser(services, logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during migration or seeding.");
                throw;
            }
        }

        private static async Task SeedAdminUser(IServiceProvider services, ILogger logger)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            var role = Environment.GetEnvironmentVariable("ADMIN_ROLE") ?? "Admin";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                logger.LogWarning("Admin seeding skipped: Missing environment variables.");
                return;
            }

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser { UserName = email, Email = email };
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));

                    await userManager.AddToRoleAsync(user, role);
                    logger.LogInformation("Admin user created successfully.");
                }
            }
        }
    }
}
