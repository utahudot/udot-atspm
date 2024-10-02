using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementImporter
{
    public static class ServiceRegistrations
    {
        public static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
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
            services.AddScoped<IMonthlyAggregationRepository, MonthlyAggregationBQRepository>(provider =>
            {
                var client = provider.GetRequiredService<BigQueryClient>();
                var datasetId = configuration["BigQuery:DatasetId"];
                var tableId = configuration["BigQuery:MonthlyAggregationTableId"];
                var logger = provider.GetRequiredService<ILogger<MonthlyAggregationBQRepository>>();
                return new MonthlyAggregationBQRepository(client, datasetId, tableId, logger);
            });
        }
    }
}
