using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpeedManagementImporter;
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
            loggingBuilder.ClearProviders();  // Clear any default providers just in case
            loggingBuilder.AddConsole();      // Add console logging explicitly
            loggingBuilder.AddDebug();        // Add debug logging (optional)
            loggingBuilder.SetMinimumLevel(LogLevel.Trace); // Set minimum log level to Trace (to capture everything)
        });

        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<IHourlySpeedRepository, HourlySpeedBQRepository>();
        services.AddScoped<ISegmentEntityRepository, SegmentEntityBQRepository>();
        services.AddScoped<ISegmentRepository, SegmentBQRepository>();
        services.AddScoped<ITempDataRepository, TempDataBQRepository>();
        services.AddScoped<IImporterFactory, ImporterFactory>();

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", configuration["GoogleApplicationCredentials"]);
        services.AddSingleton(provider =>
        {
            var projectId = configuration["BigQuery:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                throw new InvalidOperationException("ProjectId is not configured.");
            }
            return BigQueryClient.Create(projectId);
        });
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
        services.AddScoped<IImporterFactory, ImporterFactory>();

        services.AddLogging();

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application Starting");

        // Resolve the dependencies
        var hourlySpeedRepository = serviceProvider.GetRequiredService<IHourlySpeedRepository>();
        var segmentEntityRepository = serviceProvider.GetRequiredService<ISegmentEntityRepository>();
        var segmentRepository = serviceProvider.GetRequiredService<ISegmentRepository>();
        var tempDataRepository = serviceProvider.GetRequiredService<ITempDataRepository>();
        var pemsLogger = serviceProvider.GetRequiredService<ILogger<PemsDownloaderService>>();


        rootCommand.AddCommand(new Download(segmentEntityRepository, segmentRepository, hourlySpeedRepository, tempDataRepository, configuration, pemsLogger));

        return await rootCommand.InvokeAsync(args);
    }
}