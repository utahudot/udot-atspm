using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories.AggregationRepositories;
using ATSPM.Infrastructure.Repositories.ConfigurationRepositories;
using ATSPM.Infrastructure.Repositories.EventLogRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var host = CreateHostBuilder(args).Build();

        // Getting the logger from the service provider
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Application starting");

        var rootCommand = host.Services.GetRequiredService<RootCommand>();
        rootCommand.AddCommand(host.Services.GetRequiredService<ExtractCommand>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CreateCommand>());
        rootCommand.AddCommand(host.Services.GetRequiredService<DeleteConfiguration>());
        rootCommand.AddCommand(host.Services.GetRequiredService<DeleteConfigurationNew>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CopyConfiguration>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CopyConfigurationNew>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CopyArchiveLog>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CopyConfigurationNewNew>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CopyConfigurationNewPostgres>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CreateCommandPostgres>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CreateArchiveLogForTestSite>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CreateAggregationsForTestSite>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CopySpeedToPostgres>());
        rootCommand.AddCommand(host.Services.GetRequiredService<ExtractOldArchiveCommand>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CopyConfigurationFromCsv>());
        rootCommand.AddCommand(host.Services.GetRequiredService<CreateArchiveLogForTestSiteCsv>());
        //rootCommand.AddCommand(host.Services.GetRequiredService<CreateSpeedManagementAggregations>());

        // Using the host services to invoke the command
        return await host.Services.GetService<RootCommand>().InvokeAsync(args);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(configure => configure.AddConsole());
                services.AddAtspmDbContext(hostContext);
                services.AddAtspmEFEventLogRepositories();
                services.AddAtspmEFAggregationRepositories();

                services.AddTransient<ExtractCommand>();
                services.AddTransient<CreateCommand>();
                services.AddTransient<DeleteConfiguration>();
                services.AddTransient<DeleteConfigurationNew>();
                services.AddTransient<CopyConfiguration>();
                services.AddTransient<CopyConfigurationNew>();
                services.AddTransient<CopyArchiveLog>();
                services.AddTransient<CopyConfigurationNewNew>();
                services.AddTransient<CopyConfigurationNewPostgres>();
                services.AddTransient<CreateCommandPostgres>();
                services.AddTransient<CreateArchiveLogForTestSite>();
                services.AddTransient<CreateAggregationsForTestSite>();
                services.AddTransient<CopySpeedToPostgres>();
                services.AddTransient<ExtractOldArchiveCommand>();
                services.AddTransient<CopyConfigurationFromCsv>();
                services.AddTransient<CreateArchiveLogForTestSiteCsv>();
                //services.AddTransient<CreateSpeedManagementAggregations>();
                services.AddTransient<IDetectionTypeRepository, DetectionTypeEFRepository>();
                services.AddTransient<IIndianaEventLogRepository, IndianaEventLogEFRepository>();
                services.AddTransient<ILocationRepository, LocationEFRepository>();
                services.AddTransient<ILocationTypeRepository, LocationTypeEFRepository>();
                services.AddTransient<IJurisdictionRepository, JurisdictionEFRepository>();
                services.AddTransient<IPhaseCycleAggregationRepository, PhaseCycleAggregationEFRepository>();


                // Add other services
                services.AddSingleton<RootCommand>();
                // Add other command services here...
            })
         .ConfigureLogging((context, logging) =>
         {
             logging.AddEventLog(eventLogSettings =>
             {
                 // Configure EventLog settings if necessary
                 eventLogSettings.LogName = "ATSPM Data Transfer";
                 eventLogSettings.SourceName = "ArchiveLogs"; // Change this to your application's name
             });
         });
}



//using System.CommandLine;

//class Program
//{

//    static int Main(string[] args)
//    {
//        var rootCommand = new RootCommand();
//        rootCommand.AddCommand(new ExtractCommand());
//        rootCommand.AddCommand(new CreateCommand());
//        rootCommand.AddCommand(new DeleteConfiguration());
//        rootCommand.AddCommand(new DeleteConfigurationNew());
//        rootCommand.AddCommand(new CopyConfiguration());
//        rootCommand.AddCommand(new CopyConfigurationNew());
//        rootCommand.AddCommand(new CopyArchiveLog());
//        rootCommand.AddCommand(new CopyConfigurationNewNew());
//        rootCommand.AddCommand(new CopyConfigurationNewPostgres());
//        rootCommand.AddCommand(new CreateCommandPostgres());
//        rootCommand.AddCommand(new CreateArchiveLogForTestSite());


//        return rootCommand.InvokeAsync(args).Result;
//    }
//}


