using DatabaseInstaller.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;


namespace DatabaseInstaller.Services
{
    public class CopyConfigCommandHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<CopyConfigCommandConfiguration> _config;
        private readonly ILogger<CopyConfigCommandHostedService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IProductRepository _productRepository;
        private readonly ILocationRepository _locationRepository;
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
            IConfiguration appSettings,
            IProductRepository productRepository,
            ILocationRepository locationRepository)
        {
            _serviceProvider = serviceProvider;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _productRepository = productRepository;
            _locationRepository = locationRepository;
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
            MigrateProducts().Wait();
            Console.WriteLine("Data copied and bulk inserted.");
        }


        public async Task MigrateProducts()
        {
            var productsToInsert = new List<Product>
            {
                new Product { Id = 1, Manufacturer = "Econolite", Model = "ASC3" },
                new Product { Id = 2, Manufacturer = "QFree", Model = "Cobalt" },
                new Product { Id = 3, Manufacturer = "Econolite", Model = "2070" },
                new Product { Id = 4, Manufacturer = "QFree", Model = "MaxTime" },
                new Product { Id = 5, Manufacturer = "Trafficware", Model = "Trafficware" },
                new Product { Id = 6, Manufacturer = "Siemens", Model = "SEPAC" },
                new Product { Id = 7, Manufacturer = "McCain", Model = "ATC EX" },
                new Product { Id = 8, Manufacturer = "Peek", Model = "Peek" },
                new Product { Id = 9, Manufacturer = "Econolite", Model = "EOS" },
                new Product { Id = 10, Manufacturer = "Econolite", Model = "32.68.40" }
            };
            var existingProducts = _productRepository.GetList().ToList();
            foreach (var product in productsToInsert)
            {
                var existingProduct = existingProducts.FirstOrDefault(p => p.Manufacturer == product.Manufacturer && p.Model == product.Model);
                if (existingProduct == null)
                {
                    // If it doesn't exist, add the new product
                    _productRepository.Add(product);
                }
            }
        }

        public async Task SyncLocationsFromSource()
        {
            // Step 1: Fetch and transform Signals into Locations from the source (SQL Server)
            var locationsFromSource = new List<Location>();
            var command = new SqlCommand("SELECT * FROM Signal", _sqlConnection);
            _sqlConnection.Open();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var location = new Location
                    {
                        // Mapped fields
                        Id = reader.GetInt32(reader.GetOrdinal("VersionID")),             // VersionId from Signal -> Id on Location
                        LocationIdentifier = reader.GetString(reader.GetOrdinal("SignalID")), // SignalID from Signal -> LocationIdentifier on Location
                        PrimaryName = reader.GetString(reader.GetOrdinal("PrimaryName")),
                        SecondaryName = reader.GetString(reader.GetOrdinal("SecondaryName")),
                        Latitude = reader.GetDouble(reader.GetOrdinal("Latitude")),
                        Longitude = reader.GetDouble(reader.GetOrdinal("Longitude")),
                        JurisdictionId = reader.GetInt32(reader.GetOrdinal("JurisdictionID")),
                        RegionId = reader.GetInt32(reader.GetOrdinal("RegionID")),
                        Start = reader.GetDateTime(reader.GetOrdinal("Start")),

                        // Fields with ?
                        Note = reader.GetString(reader.GetOrdinal("Note")),  // Unsure if the Signal.Note field is applicable
                        ChartEnabled = true,  // ? (Assumed default value, let me know if this needs to be determined by logic)
                        PedsAre1to1 = reader.GetBoolean(reader.GetOrdinal("Pedsare1to1")),  // PedsAre1to1 from Signal (is this correct?)

                        // Fields from Location that I don't have mappings for:
                        VersionAction = LocationVersionActions.Updated,  // ? (Is there a field in Signal that should map to this?)
                        Devices = new List<Device>(),                    // ? (Will need to be populated separately, I assume)
                        Approaches = new List<Approach>(),               // ? (Should be populated separately if needed)
                        Areas = new List<Area>()                         // ? (Need clarification on where to map this from)
                    };
                    locationsFromSource.Add(location);
                }
            }
            _sqlConnection.Close();

            // Step 2: Fetch existing locations from the target database
            var existingLocations = await _newDbContext.Locations.ToListAsync();

            // Step 3: Compare and update locations
            foreach (var sourceLocation in locationsFromSource)
            {
                var targetLocation = existingLocations.FirstOrDefault(l => l.Id == sourceLocation.Id);

                if (targetLocation != null)
                {
                    // Compare fields and update if different
                    if (targetLocation.LocationIdentifier != sourceLocation.LocationIdentifier)
                        targetLocation.LocationIdentifier = sourceLocation.LocationIdentifier;

                    if (targetLocation.PrimaryName != sourceLocation.PrimaryName)
                        targetLocation.PrimaryName = sourceLocation.PrimaryName;

                    if (targetLocation.SecondaryName != sourceLocation.SecondaryName)
                        targetLocation.SecondaryName = sourceLocation.SecondaryName;

                    if (targetLocation.Latitude != sourceLocation.Latitude)
                        targetLocation.Latitude = sourceLocation.Latitude;

                    if (targetLocation.Longitude != sourceLocation.Longitude)
                        targetLocation.Longitude = sourceLocation.Longitude;

                    if (targetLocation.JurisdictionId != sourceLocation.JurisdictionId)
                        targetLocation.JurisdictionId = sourceLocation.JurisdictionId;

                    if (targetLocation.RegionId != sourceLocation.RegionId)
                        targetLocation.RegionId = sourceLocation.RegionId;

                    if (targetLocation.Note != sourceLocation.Note)  // ?
                        targetLocation.Note = sourceLocation.Note;

                    if (targetLocation.ChartEnabled != sourceLocation.ChartEnabled)  // ?
                        targetLocation.ChartEnabled = sourceLocation.ChartEnabled;

                    if (targetLocation.PedsAre1to1 != sourceLocation.PedsAre1to1)
                        targetLocation.PedsAre1to1 = sourceLocation.PedsAre1to1;

                    if (targetLocation.Start != sourceLocation.Start)
                        targetLocation.Start = sourceLocation.Start;

                    // Other fields such as Devices, Approaches, and Areas can be populated separately
                }
            }

            // Step 4: Add new locations that exist in the source but not in the target
            foreach (var sourceLocation in locationsFromSource)
            {
                if (!existingLocations.Any(l => l.Id == sourceLocation.Id))
                {
                    // Add new location
                    _newDbContext.Locations.Add(sourceLocation);
                }
            }

            // Step 5: Remove locations that no longer exist in the source
            foreach (var targetLocation in existingLocations)
            {
                if (!locationsFromSource.Any(l => l.Id == targetLocation.Id))
                {
                    // Remove location
                    _newDbContext.Locations.Remove(targetLocation);
                }
            }

            // Save all changes to the database
            await _newDbContext.SaveChangesAsync();
        }



        //   private void FindNewAndDeletedFromGeneralConfiguration()
        //   {
        //       using (SqlConnection sourceConnection = new SqlConnection(_config.Value.Source))
        //       {
        //           sourceConnection.Open();
        //       }
        //   }

        //   private async Task<List<Dictionary<string, object>>> GetSourceDataAsync(string tableName, Dictionary<string, string> columnMappings)
        //   {
        //       var sourceData = new List<Dictionary<string, object>>();

        //       using (SqlConnection sourceConnection = new SqlConnection(_config.Value.Source))
        //       {
        //           await sourceConnection.OpenAsync();
        //           var columns = string.Join(", ", columnMappings.Keys); // Use the column mappings to select relevant fields
        //           string query = $"SELECT {columns} FROM {tableName}";

        //           using (SqlCommand command = new SqlCommand(query, sourceConnection))
        //           {
        //               using (SqlDataReader reader = await command.ExecuteReaderAsync())
        //               {
        //                   while (await reader.ReadAsync())
        //                   {
        //                       var row = new Dictionary<string, object>();
        //                       foreach (var column in columnMappings.Keys)
        //                       {
        //                           row[column] = reader[column];
        //                       }
        //                       sourceData.Add(row);
        //                   }
        //               }
        //           }
        //       }

        //       return sourceData;
        //   }

        //   private async Task<List<Dictionary<string, object>>> GetTargetDataAsync<TEntity>(DbContext dbContext, string tableName, Dictionary<string, string> columnMappings)
        //where TEntity : class
        //   {
        //       var targetData = new List<Dictionary<string, object>>();
        //       var dbSet = dbContext.Set<TEntity>(); // Get the corresponding DbSet

        //       foreach (var entity in await dbSet.ToListAsync())
        //       {
        //           var row = new Dictionary<string, object>();
        //           foreach (var column in columnMappings.Values)
        //           {
        //               var value = entity.GetType().GetProperty(column)?.GetValue(entity);
        //               row[column] = value;
        //           }
        //           targetData.Add(row);
        //       }

        //       return targetData;
        //   }


        //   private void CompareData(List<Dictionary<string, object>> sourceData, List<Dictionary<string, object>> targetData, string idColumn)
        //   {
        //       var sourceIds = sourceData.Select(row => row[idColumn].ToString()).ToHashSet();
        //       var targetIds = targetData.Select(row => row[idColumn].ToString()).ToHashSet();

        //       var newItems = sourceIds.Except(targetIds).ToList();
        //       var deletedItems = targetIds.Except(sourceIds).ToList();

        //       _logger.LogInformation($"New items in source for {idColumn}: {string.Join(", ", newItems)}");
        //       _logger.LogInformation($"Deleted items in target for {idColumn}: {string.Join(", ", deletedItems)}");
        //   }

        //   public async Task CompareTablesAsync()
        //   {
        //       using (var scope = _serviceProvider.CreateScope())
        //       {
        //           var dbContext = scope.ServiceProvider.GetRequiredService<ConfigContext>(); // Target database context

        //           foreach (var tableMapping in _tableMappings)
        //           {
        //               var tableName = tableMapping.Key;
        //               var entityName = tableMapping.Value;

        //               // Get column mappings
        //               var columnMappings = _columnMappings[tableName];

        //               // Get source data (SQL Server)
        //               var sourceData = await GetSourceDataAsync(tableName, columnMappings);

        //               // Explicitly specify the entity type for the target data query
        //               if (entityName == "Jurisdictions")
        //               {
        //                   var targetData = await GetTargetDataAsync<Jurisdiction>(dbContext, tableName, columnMappings);
        //                   var idColumn = columnMappings.FirstOrDefault(cm => cm.Key.Contains("Id")).Key;
        //                   if (idColumn != null)
        //                   {
        //                       CompareData(sourceData, targetData, idColumn);
        //                   }
        //               }
        //               // Add more conditions here for other entities (tables) like Products, DeviceConfigurations, etc.
        //           }
        //       }
        //   }





        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }


}
