using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpeedEventEmitter.Services;                                              // Detector
using Utah.Udot.Atspm.Infrastructure.Extensions;

namespace SpeedEventEmitter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                 .ConfigureAppConfiguration((hostingCtx, config) =>
                 {
                     config.AddUserSecrets<Program>(optional: true);
                 })
                // 3) Finally, let command-line args trump everything
                .ConfigureAppConfiguration((hostingCtx, config) =>
                {
                    config.AddCommandLine(args);
                })
                .ConfigureServices((ctx, services) =>
                {
                    // --- logging & configuration ---
                    services.AddLogging(cfg => cfg.AddConsole());

                    // --- EF Core DbContext for your config store ---
                    services.AddAtspmDbContext(ctx);
                    services.AddAtspmEFConfigRepositories();

                    // --- emitter hosted service ---
                    services.AddHostedService<SpeedEmitterService>();
                    //services.AddHostedService<TestDataSeederService>();
                })
                .RunConsoleAsync();
        }
    }


}
