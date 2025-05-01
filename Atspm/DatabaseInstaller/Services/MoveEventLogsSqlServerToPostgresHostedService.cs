#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/MoveEventLogsSqlServerToPostgresHostedService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using global::DatabaseInstaller.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;


namespace DatabaseInstaller.Services
{
    public class MoveEventLogsSqlServerToPostgresHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TransferCommandConfiguration _config;
        private readonly ILogger<UpdateCommandHostedService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IEventLogRepository _eventLogRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IndianaEventLogEFRepository _indianaEventLogEFRepository;

        public MoveEventLogsSqlServerToPostgresHostedService(
            IServiceProvider serviceProvider,
            IOptions<TransferCommandConfiguration> config,
            ILogger<UpdateCommandHostedService> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IEventLogRepository eventLogRepository,
            ILocationRepository locationRepository
            //IndianaEventLogEFRepository _indianaEventLogEFRepository
            )
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _eventLogRepository = eventLogRepository;
            _locationRepository = locationRepository;
            //_indianaEventLogEFRepository = _indianaEventLogEFRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                //    using var scope = _serviceProvider.CreateScope();
                //    var _serviceProvider = scope.ServiceProvider;

                //    // SQL Server DbContext to read logs
                //    var sqlOptions = new DbContextOptionsBuilder<EventLogContext>()
                //        .UseSqlServer(_config.Value.Source)
                //        .Options;

                //    using var sqlServerContext = new EventLogContext(sqlOptions);
                //    var sqlSeverRepository = new IndianaEventLogEFRepository(sqlServerContext, _serviceProvider.GetService<ILogger<IndianaEventLogEFRepository>>());
                //    var sqltestSeverRepository = new EventLogEFRepository(sqlServerContext, _serviceProvider.GetService<ILogger<EventLogEFRepository>>());


                // PostgreSQL DbContext to write logs
                //var postgresOptions = new DbContextOptionsBuilder<EventLogContext>()
                //    .UseNpgsql(_config.Value.Target)
                //    .Options;

                //using var postgresContext = new EventLogContext(postgresOptions);
                //var postgresSeverRepository = new IndianaEventLogEFRepository(postgresContext, _serviceProvider.GetService<ILogger<IndianaEventLogEFRepository>>());
                //                var locations = new List<string>
                //               {
                //                    "2122",
                //"2123",
                //"2124",
                //"2125",
                //"2126",
                //"2127",
                //"2128",
                //"2129",
                //"2132",
                //"2133",
                //"2136",
                //"2137",
                //"2138",
                //"2139",
                //"2140",
                //"2141",
                //"2142",
                //"2143",
                //"2144",
                //"2145",
                //"2146",
                //"2147",
                //"2148",
                //"2149",
                //"2150",
                //"2151",
                //"2155",
                //"2156",
                //"2157",
                //"2302",
                //"2303",
                //"2306",
                //"2307",
                //"2308",
                //"2309",
                //"2310",
                //"2311",
                //"2312",
                //"2313",
                //"2316",
                //"2317",
                //"2318",
                //"2319",
                //"2324",
                //"2325",
                //"2326",
                //"2327",
                //"2328",
                //"2329",
                //"2335",
                //"2340",
                //"2341",
                //"2347",
                //"2392",
                //"2394",
                //"2395",
                //"2396",
                //"2397",
                //"2700",
                //"2702",
                //"2703",
                //"2704",
                //"2705",
                //"2706",
                //"2707",
                //"2708",
                //"2709",
                //"2710",
                //"2712",
                //"2713",
                //"2718",
                //"2719",
                //"2720",
                //"2721",
                //"2722",
                //"2723",
                //"2724",
                //"2725",
                //"2726",
                //"2727",
                //"2728",
                //"2729",
                //"2734",
                //"2798"
                //                };

                var locations = _locationRepository.GetList()
                    .Include(l => l.Devices)
                    .Where(l => l.Devices.Select(d => d.DeviceType).ToList().Contains(Utah.Udot.Atspm.Data.Enums.DeviceTypes.RampController))
                    .Select(l => l.LocationIdentifier)
                    .ToList();
                foreach (var location in locations)
                {
                    //create a scope to run in using
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // SQL Server DbContext to read logs
                        var sqlOptions = new DbContextOptionsBuilder<EventLogContext>()
                            .UseSqlServer(_config.Source)
                            .Options;

                        using var sqlServerContext = new EventLogContext(sqlOptions);
                        //var sqlSeverRepository = new IndianaEventLogEFRepository(sqlServerContext, scope.ServiceProvider.GetService<ILogger<IndianaEventLogEFRepository>>());
                        var sqltestSeverRepository = new EventLogEFRepository(sqlServerContext, scope.ServiceProvider.GetService<ILogger<EventLogEFRepository>>());
                        var context = scope.ServiceProvider.GetService<EventLogContext>();
                        var postgresSeverRepository = new EventLogEFRepository(context, scope.ServiceProvider.GetService<ILogger<EventLogEFRepository>>());


                        try
                        {
                            Console.WriteLine($"Getting logs for {location}...");
                            var logs = sqltestSeverRepository.GetArchivedEvents(
                                location,
                                _config.Start,
                                _config.End);
                            Console.WriteLine($"Logs for {location} retrieved");
                            Console.WriteLine($"Saving logs for {location}...");
                            postgresSeverRepository.AddRange(logs);
                            Console.WriteLine($"Logs for {location} Saved");
                        }
                        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    }
                }

                // Fetch logs from SQL Server
                //var logs = sqlSeverRepository.GetEventsBetweenDates("4613", Convert.ToDateTime("2024-09-24"), Convert.ToDateTime("2024-09-25"));
                //List<CompressedEventLogs<IndianaEvent>> archiveLogs = new List<CompressedEventLogs<IndianaEvent>>();
                //archiveLogs.Add(new CompressedEventLogs<IndianaEvent>
                //{
                //     ArchiveDate= DateOnly.FromDateTime(Convert.ToDateTime("2024-09-24")),
                //      DataType
                //});
                //_indianaEventLogEFRepository.AddRange(logs);


                _logger.LogInformation("Event logs moved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while moving event logs: {Exception}", ex);
            }
            finally
            {
                _logger.LogInformation("Shutting down the application after moving logs.");
                _hostApplicationLifetime.StopApplication();
            }
        }



        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }


}
