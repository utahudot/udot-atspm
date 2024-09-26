using global::DatabaseInstaller.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;


namespace DatabaseInstaller.Services
{
    public class CopyConfigCommandHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<CopyConfigCommandConfiguration> _config;
        private readonly ILogger<CopyConfigCommandHostedService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly Dictionary<string, string> _tableMappings;
        private readonly Dictionary<string, Dictionary<string, string>> _columnMappings;
        private readonly Dictionary<string, string> _locationTableMappings;
        private readonly Dictionary<string, string> _generalQueries;
        private readonly Dictionary<string, string> _locationQueries;

        public CopyConfigCommandHostedService(
            IServiceProvider serviceProvider,
            IOptions<CopyConfigCommandConfiguration> config,
            ILogger<CopyConfigCommandHostedService> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IConfiguration appSettings)
        {
            _serviceProvider = serviceProvider;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _tableMappings = appSettings.GetSection("TableMappings").Get<Dictionary<string, string>>();
            _columnMappings = appSettings.GetSection("ColumnMappings").Get<Dictionary<string, Dictionary<string, string>>>();
            _locationTableMappings = appSettings.GetSection("LocationTableMappings").Get<Dictionary<string, string>>();
            _generalQueries = appSettings.GetSection("GeneralQueries").Get<Dictionary<string, string>>();
            _locationQueries = appSettings.GetSection("LocationQueries").Get<Dictionary<string, string>>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Source is {_config.Value.Source}");
            Console.WriteLine($"Target is {_config.Value.Target}");
            FindNewAndDeletedFromGeneralConfiguration();
            Console.WriteLine("Data copied and bulk inserted.");
        }


        public async Task MigrateProducts()
        {
            //var command = new SqlCommand("SELECT * FROM ControllerType", );
            //_sqlConnection.Open();
            //using (var reader = await command.ExecuteReaderAsync())
            //{
            //    while (await reader.ReadAsync())
            //    {
            //        var product = new Product
            //        {
            //            Manufacturer = reader.GetString(reader.GetOrdinal("Description")), // Mapping Manufacturer from ControllerType.Description
            //            Model = reader.GetString(reader.GetOrdinal("Description")),        // Mapping Model from ControllerType.Description (or another field?)
            //            WebPage = "?",                                                     // Unclear how to map WebPage
            //            Notes = "?"                                                        // Unclear how to map Notes
            //        };
            //        _newDbContext.Products.Add(product);
            //    }
            //}
            //_sqlConnection.Close();
            //await _newDbContext.SaveChangesAsync();
        }



        private void FindNewAndDeletedFromGeneralConfiguration()
        {
            using (SqlConnection sourceConnection = new SqlConnection(_config.Value.Source))
            {
                sourceConnection.Open();
            }
        }

        private async Task<List<Dictionary<string, object>>> GetSourceDataAsync(string tableName, Dictionary<string, string> columnMappings)
        {
            var sourceData = new List<Dictionary<string, object>>();

            using (SqlConnection sourceConnection = new SqlConnection(_config.Value.Source))
            {
                await sourceConnection.OpenAsync();
                var columns = string.Join(", ", columnMappings.Keys); // Use the column mappings to select relevant fields
                string query = $"SELECT {columns} FROM {tableName}";

                using (SqlCommand command = new SqlCommand(query, sourceConnection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>();
                            foreach (var column in columnMappings.Keys)
                            {
                                row[column] = reader[column];
                            }
                            sourceData.Add(row);
                        }
                    }
                }
            }

            return sourceData;
        }

        private async Task<List<Dictionary<string, object>>> GetTargetDataAsync<TEntity>(DbContext dbContext, string tableName, Dictionary<string, string> columnMappings)
     where TEntity : class
        {
            var targetData = new List<Dictionary<string, object>>();
            var dbSet = dbContext.Set<TEntity>(); // Get the corresponding DbSet

            foreach (var entity in await dbSet.ToListAsync())
            {
                var row = new Dictionary<string, object>();
                foreach (var column in columnMappings.Values)
                {
                    var value = entity.GetType().GetProperty(column)?.GetValue(entity);
                    row[column] = value;
                }
                targetData.Add(row);
            }

            return targetData;
        }


        private void CompareData(List<Dictionary<string, object>> sourceData, List<Dictionary<string, object>> targetData, string idColumn)
        {
            var sourceIds = sourceData.Select(row => row[idColumn].ToString()).ToHashSet();
            var targetIds = targetData.Select(row => row[idColumn].ToString()).ToHashSet();

            var newItems = sourceIds.Except(targetIds).ToList();
            var deletedItems = targetIds.Except(sourceIds).ToList();

            _logger.LogInformation($"New items in source for {idColumn}: {string.Join(", ", newItems)}");
            _logger.LogInformation($"Deleted items in target for {idColumn}: {string.Join(", ", deletedItems)}");
        }

        public async Task CompareTablesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ConfigContext>(); // Target database context

                foreach (var tableMapping in _tableMappings)
                {
                    var tableName = tableMapping.Key;
                    var entityName = tableMapping.Value;

                    // Get column mappings
                    var columnMappings = _columnMappings[tableName];

                    // Get source data (SQL Server)
                    var sourceData = await GetSourceDataAsync(tableName, columnMappings);

                    // Explicitly specify the entity type for the target data query
                    if (entityName == "Jurisdictions")
                    {
                        var targetData = await GetTargetDataAsync<Jurisdiction>(dbContext, tableName, columnMappings);
                        var idColumn = columnMappings.FirstOrDefault(cm => cm.Key.Contains("Id")).Key;
                        if (idColumn != null)
                        {
                            CompareData(sourceData, targetData, idColumn);
                        }
                    }
                    // Add more conditions here for other entities (tables) like Products, DeviceConfigurations, etc.
                }
            }
        }





        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }


}
