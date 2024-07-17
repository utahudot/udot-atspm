using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpeedManagementDataDownloader.Business.Common.ImporterFactory;
using SpeedManagementDataDownloader.Common.EntityTable;
using SpeedManagementDataDownloader.Common.HourlySpeeds;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Console.WriteLine($"ProjectId: {configuration["BigQuery:ProjectId"]}");
            Console.WriteLine($"DatasetId: {configuration["BigQuery:DatasetId"]}");
            Console.WriteLine($"HourlySpeedTableId: {configuration["BigQuery:HourlySpeedTableId"]}");
            Console.WriteLine($"RouteEntityTableId: {configuration["BigQuery:RouteEntityTableId"]}");
            Console.WriteLine($"GoogleApplicationCredentials: {configuration["GoogleApplicationCredentials"]}");
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
                return new HourlySpeedBQRepository(client, datasetId, tableId);
            });
            services.AddScoped<IRouteEntityTableRepository, RouteEntityTableRepository>(provider =>
            {
                var client = provider.GetRequiredService<BigQueryClient>();
                var datasetId = configuration["BigQuery:DatasetId"];
                var tableId = configuration["BigQuery:RouteEntityTableId"];
                return new RouteEntityTableRepository(client, datasetId, tableId);
            });
            services.AddScoped<IImporterFactory, ImporterFactory>();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the dependencies
            var hourlySpeedRepository = serviceProvider.GetRequiredService<IHourlySpeedRepository>();
            var routeEntityTableRepository = serviceProvider.GetRequiredService<IRouteEntityTableRepository>();


            rootCommand.AddCommand(new Download(routeEntityTableRepository, hourlySpeedRepository));

            return await rootCommand.InvokeAsync(args);
        }
    }
}
