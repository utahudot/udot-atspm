using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Data;                                                   // your ConfigContext
using Utah.Udot.Atspm.Data.Enums;                                              // DeviceTypes, etc.
using Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories;    // IDeviceRepository, DeviceConfigurationEFRepository
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Microsoft.Extensions.Configuration;
using SpeedEventEmitter.Services;                                              // Detector

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
