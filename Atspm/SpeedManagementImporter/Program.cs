﻿using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Infrastructure.Repositories.SpeedManagementRepositories;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpeedManagementImporter;
using System.CommandLine;

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

        // Resolve the dependencies
        var hourlySpeedRepository = serviceProvider.GetRequiredService<IHourlySpeedRepository>();
        var routeEntityTableRepository = serviceProvider.GetRequiredService<ISegmentEntityRepository>();
        var tempDataRepository = serviceProvider.GetRequiredService<ITempDataRepository>();


        rootCommand.AddCommand(new Download(routeEntityTableRepository, hourlySpeedRepository, tempDataRepository, configuration));

        return await rootCommand.InvokeAsync(args);
    }
}