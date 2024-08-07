using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Infrastructure.Repositories.SpeedManagementRepositories;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SpeedManagementImporter
{
    public static class ServiceRegistrations
    {
        public static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IHourlySpeedRepository, HourlySpeedBQRepository>(provider =>
            {
                var client = provider.GetRequiredService<BigQueryClient>();
                var datasetId = configuration["BigQuery:DatasetId"];
                var tableId = configuration["BigQuery:HourlySpeedTableId"];
                var logger = provider.GetRequiredService<ILogger<HourlySpeedBQRepository>>();
                return new HourlySpeedBQRepository(client, datasetId, tableId, logger);
            });
        }
    }
}
