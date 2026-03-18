#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/ImportLocationsHostedService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using CsvHelper;
using CsvHelper.Configuration;
using DatabaseInstaller.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;

namespace DatabaseInstaller.Services
{
    public class ImportLocationsHostedService : IHostedService
{
    private readonly ILogger<ImportLocationsHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ImportLocationsCommandConfiguration _config;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public ImportLocationsHostedService(
        ILogger<ImportLocationsHostedService> logger,
        IServiceProvider serviceProvider,
        IOptions<ImportLocationsCommandConfiguration> config,
        IHostApplicationLifetime hostApplicationLifetime
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config.Value;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting location import from {File}", _config.File);

        if (!File.Exists(_config.File))
        {
            _logger.LogError("File not found: {File}", _config.File);
            _hostApplicationLifetime.StopApplication();
            return;
        }

        var locations = ReadCsvFile(_config.File);
        _logger.LogInformation("Read {Count} locations from CSV", locations.Count);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigContext>();

        if (!string.IsNullOrEmpty(_config.ConfigConnection))
        {
            context.Database.SetConnectionString(_config.ConfigConnection);
        }

        if (_config.Delete)
        {
            _logger.LogInformation("Deleting existing locations...");
            var existingLocations = await context.Locations.ToListAsync(cancellationToken);
            context.Locations.RemoveRange(existingLocations);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Deleted {Count} existing locations", existingLocations.Count);
        }

        var locationType = await context.LocationTypes
            .FirstOrDefaultAsync(lt => lt.Name.ToLower() == _config.LocationType.ToLower(), cancellationToken);

            if (locationType == null)
        {
            _logger.LogWarning("Location type '{LocationType}' not found, using first available", _config.LocationType);
            locationType = await context.LocationTypes.FirstOrDefaultAsync(cancellationToken);
        }

        int imported = 0;
        int skipped = 0;
        int updated = 0;

        foreach (var loc in locations)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loc.LocationIdentifier))
                {
                    _logger.LogWarning("Skipping row with empty LocationIdentifier");
                    skipped++;
                    continue;
                }

                var existing = await context.Locations
                    .FirstOrDefaultAsync(l => l.LocationIdentifier == loc.LocationIdentifier, cancellationToken);

                if (existing != null)
                {
                    _logger.LogInformation("Location {Identifier} already exists, updating", loc.LocationIdentifier);
                    existing.PrimaryName = loc.PrimaryName;
                    existing.SecondaryName = loc.SecondaryName;
                    existing.Latitude = loc.Latitude;
                    existing.Longitude = loc.Longitude;
                    existing.Note = "";
                    context.Locations.Update(existing);
                    updated++;
                }
                else
                {
                    loc.LocationTypeId = locationType?.Id ?? 1;
                    loc.Start = DateTime.UtcNow;
                    await context.Locations.AddAsync(loc, cancellationToken);
                    imported++;
                }

                if ((imported + updated) % 50 == 0)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing location {Identifier}", loc.LocationIdentifier);
                skipped++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Import complete. Added: {Added}, Updated: {Updated}, Skipped: {Skipped}", imported, updated, skipped);
        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private List<Location> ReadCsvFile(string filePath)
    {
        var locations = new List<Location>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        while (csv.Read())
        {
            var street1 = GetField(csv, headers, "Street Name 1");
            var street2 = GetField(csv, headers, "Street Name 2");

            var location = new Location
            {
                LocationIdentifier = GetField(csv, headers, "Asset"),
                PrimaryName = street1,
                SecondaryName = street2,
                Latitude = GetDoubleField(csv, headers, "Lat"),
                Longitude = GetDoubleField(csv, headers, "Lon"),
                ChartEnabled = true,
                Note = ""
            };

            locations.Add(location);
        }

        return locations;
    }

    private string GetField(CsvReader csv, string[] headers, string fieldName)
    {
        var index = Array.FindIndex(headers, h => h.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        if (index < 0) return null;
        return csv.GetField(index);
    }

    private double GetDoubleField(CsvReader csv, string[] headers, string fieldName)
    {
        var value = GetField(csv, headers, fieldName);
        if (double.TryParse(value, out var result)) return result;
        return 0;
    }
}
}