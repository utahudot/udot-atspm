using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpeedManagementImporter.Services.Atspm;
using SpeedManagementImporter.Services.Clearguide;
using SpeedManagementImporter.Services.Pems;
using System.CommandLine;
using Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand();

        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        // Configure logging services explicitly
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();  // Clear any default providers
            loggingBuilder.AddConsole();      // Add console logging
            loggingBuilder.AddDebug();        // Add debug logging (optional)
            loggingBuilder.SetMinimumLevel(LogLevel.Trace); // Set minimum log level to Trace
        });

        // Register configuration as IConfiguration
        services.AddSingleton<IConfiguration>(configuration);

        // Register download services
        services.AddScoped<PemsDownloaderService>();
        services.AddScoped<AtspmDownloaderService>();
        services.AddScoped<ClearguideFileDownloaderService>();

        // Set Google Cloud Credentials
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", configuration["GoogleApplicationCredentials"]);

        // Add BigQueryClient as a singleton
        services.AddSingleton(provider =>
        {
            var projectId = configuration["BigQuery:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                throw new InvalidOperationException("ProjectId is not configured.");
            }
            return BigQueryClient.Create(projectId);
        });

        // Register repositories with BigQuery dependencies
        services.AddScoped<IHourlySpeedRepository, HourlySpeedBQRepository>(provider =>
        {
            var client = provider.GetRequiredService<BigQueryClient>();
            var datasetId = configuration["BigQuery:DatasetId"];
            var tableId = configuration["BigQuery:HourlySpeedTableId"];
            var logger = provider.GetRequiredService<ILogger<HourlySpeedBQRepository>>();
            return new HourlySpeedBQRepository(client, datasetId, tableId, logger);
        });

        services.AddScoped<ISegmentEntityRepository, SegmentEntityBQRepository>(provider =>
        {
            var client = provider.GetRequiredService<BigQueryClient>();
            var datasetId = configuration["BigQuery:DatasetId"];
            var tableId = configuration["BigQuery:RouteEntityTableId"];
            var logger = provider.GetRequiredService<ILogger<SegmentEntityBQRepository>>();
            return new SegmentEntityBQRepository(client, datasetId, tableId, logger);
        });

        services.AddScoped<ISegmentRepository, SegmentBQRepository>(provider =>
        {
            var client = provider.GetRequiredService<BigQueryClient>();
            var datasetId = configuration["BigQuery:DatasetId"];
            var tableId = configuration["BigQuery:RouteTableId"];
            var projectId = configuration["BigQuery:ProjectId"];
            var logger = provider.GetRequiredService<ILogger<SegmentBQRepository>>();
            return new SegmentBQRepository(client, datasetId, tableId, projectId, logger);
        });

        services.AddScoped<ITempDataRepository, TempDataBQRepository>(provider =>
        {
            var client = provider.GetRequiredService<BigQueryClient>();
            var datasetId = configuration["BigQuery:DatasetId"];
            var tableId = configuration["BigQuery:TempDataTableId"];
            var logger = provider.GetRequiredService<ILogger<TempDataBQRepository>>();
            return new TempDataBQRepository(client, datasetId, tableId, logger);
        });

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application Starting");

        // Resolve the dependencies
        var pemsDownloader = serviceProvider.GetRequiredService<PemsDownloaderService>();
        var atspmDownloader = serviceProvider.GetRequiredService<AtspmDownloaderService>();
        var clearguideDownloader = serviceProvider.GetRequiredService<ClearguideFileDownloaderService>();

        // Add the download command
        rootCommand.AddCommand(new Download(pemsDownloader, atspmDownloader, clearguideDownloader));

        return await rootCommand.InvokeAsync(args);
    }
}
