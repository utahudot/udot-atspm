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

using DatabaseInstaller.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace DatabaseInstaller.Services
{
    public class MoveEventLogsSqlServerToPostgresHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MoveEventLogsSqlServerToPostgresHostedService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly TransferCommandConfiguration _config;
        private readonly ILocationRepository _locationRepository;

        public MoveEventLogsSqlServerToPostgresHostedService(
            IServiceProvider serviceProvider,
            IOptions<TransferCommandConfiguration> config,
            ILogger<MoveEventLogsSqlServerToPostgresHostedService> logger,
            IHostApplicationLifetime lifetime,
            ILocationRepository locationRepository)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _logger = logger;
            _lifetime = lifetime;
            _locationRepository = locationRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var locations = GetTargetLocations();
                foreach (var location in locations)
                {
                    await ProcessLocationAsync(location, cancellationToken);
                }

                _logger.LogInformation("Event logs moved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while moving event logs.");
            }
            finally
            {
                _lifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private List<Location> GetTargetLocations()
        {
            var query = _locationRepository
                .GetList()
                .Include(l => l.Devices)
                .AsQueryable();

            if (_config.Device.HasValue)
            {
                query = query.Where(l => l.Devices
                    .Any(d => d.DeviceType == (DeviceTypes)_config.Device));
            }

            if (!string.IsNullOrEmpty(_config.Locations))
            {
                var locationIdentifiers = _config.Locations.Split(',', StringSplitOptions.RemoveEmptyEntries);
                query = query
                    .Where(l => locationIdentifiers.Contains(l.LocationIdentifier));
            }

            return query
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(l => l.LocationIdentifier)
                .Select(g => g.OrderByDescending(l => l.Start).First())
                .ToList();
        }

        private async Task ProcessLocationAsync(Location location, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing location {LocationId}", location.LocationIdentifier);

            for (var date = _config.Start.Date;
                 date <= _config.End.Date;
                 date = date.AddDays(1))
            {
                await ProcessDateAsync(location, date, cancellationToken);
            }
        }

        private async Task ProcessDateAsync(Location location, DateTime date, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("I removed the archive date and didn't know how to change this -CB");
            //using var scope = _serviceProvider.CreateScope();
            //var sqlContext = CreateSqlContext();
            //var sqlRepo = new IndianaEventLogEFRepository(sqlContext, scope.ServiceProvider.GetRequiredService<ILogger<IndianaEventLogEFRepository>>());

            //var allLogs = sqlRepo.GetList()
            //    .Where(l => l.LocationIdentifier == location.LocationIdentifier && l.ArchiveDate == DateOnly.FromDateTime(date))
            //    .AsNoTracking()
            //    .AsEnumerable()
            //    .SelectMany(m => m.Data)
            //    .FromSpecification(new EventLogSpecification(location.LocationIdentifier, date, date.AddDays(1).AddMilliseconds(-1)))
            //    .Cast<IndianaEvent>()
            //    .ToList();

            //for (int hour = 0; hour < 24; hour++)
            //{
            //    var hourlyLogs = allLogs.Where(l => l.Timestamp.Hour == hour).ToList();
            //    if (!hourlyLogs.Any()) continue;

            //    await SaveHourlyLogsAsync(location, date, hourlyLogs, scope, cancellationToken);
            //}
        }

        private async Task SaveHourlyLogsAsync(
            Location location,
            DateTime date,
            List<IndianaEvent> logs,
            IServiceScope scope,
            CancellationToken cancellationToken)
        {
            try
            {
                var postgresContext = scope.ServiceProvider.GetRequiredService<EventLogContext>();
                var pgRepo = new IndianaEventLogEFRepository(postgresContext, scope.ServiceProvider.GetRequiredService<ILogger<IndianaEventLogEFRepository>>());

                var deviceId = location.Devices
                    .FirstOrDefault(d => d.DeviceType == DeviceTypes.RampController)
                    ?.Id;
                if (deviceId == null)
                {
                    _logger.LogWarning("No SignalController device for {LocationId}", location.LocationIdentifier);
                    return;
                }

                var archiveLog = new CompressedEventLogs<IndianaEvent>
                {
                    //ArchiveDate = DateOnly.FromDateTime(date),
                    LocationIdentifier = location.LocationIdentifier,
                    DeviceId = deviceId.Value,
                    Data = logs
                };

                pgRepo.Add(archiveLog);
                await postgresContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Saved {Count} logs for {LocationId} at hour {Hour}",
                    logs.Count,
                    location.LocationIdentifier,
                    date.Hour);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save logs for {LocationId} on {Date}", location.LocationIdentifier, date);
            }
        }

        private EventLogContext CreateSqlContext()
        {
            var options = new DbContextOptionsBuilder<EventLogContext>()
                .UseSqlServer(_config.Source)
                .Options;

            return new EventLogContext(options);
        }
    }
}
