#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/ExportLogsUtilityHostedService.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Hosted service responsible for extracting controller event logs to disk in parallel.
    /// </summary>
    /// <param name="log">The logger instance.</param>
    /// <param name="serviceProvider">The service scope factory provider.</param>
    /// <param name="options">The event log extraction options.</param>
    public class ExtractEventLogHostedService(ILogger<ExtractEventLogHostedService> log, IServiceScopeFactory serviceProvider, IOptions<EventLogExtractConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly ILogger<ExtractEventLogHostedService> _log = log;
        private readonly IServiceScopeFactory _scopeFactory = serviceProvider;
        private readonly IOptions<EventLogExtractConfiguration> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch = null, CancellationToken cancellationToken = default)
        {
            var eventRepo = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();

            var dates = _options.Value.Dates?.ToList() ?? new List<DateTime>();
            var tsFormat = _options.Value.DateTimeFormat;
            var basePath = _options.Value.Path;
            var includedLocations = _options.Value.Included?.ToList();
            var excludedLocations = _options.Value.Excluded?.ToList();
            var fileFormat = _options.Value.FileFormat?.ToLowerInvariant() ?? "csv";
            var parallelProcesses = _options.Value.ParallelProcesses > 0 ? _options.Value.ParallelProcesses : 1;

            Console.WriteLine($"Exporting event logs to {basePath} in {fileFormat} format with timestamp format {tsFormat} (Max Degree of Parallelism: {parallelProcesses})");

            List<string> locations;
            if (includedLocations != null && includedLocations.Count > 0)
            {
                locations = includedLocations;
            }
            else
            {
                locations = await eventRepo.GetList()
                    .Select(s => s.LocationIdentifier)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }

            if (excludedLocations != null && excludedLocations.Count > 0)
            {
                var excludedSet = new HashSet<string>(excludedLocations);
                locations = locations.Where(l => !excludedSet.Contains(l)).ToList();
            }

            var workItems = (from d in dates
                             from l in locations
                             select (Date: d, Location: l)).ToList();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = parallelProcesses,
                CancellationToken = cancellationToken
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            await Parallel.ForEachAsync(workItems, parallelOptions, async (item, token) =>
            {
                var (d, l) = item;

                Console.WriteLine($"Processing {l} for {d:d}");

                using var taskScope = _scopeFactory.CreateScope();
                var taskRepo = taskScope.ServiceProvider.GetRequiredService<IEventLogRepository>();

                var compressedEvents = await taskRepo.GetData<IndianaEvent>(l, d, d.AddDays(1).AddTicks(-1))
                    .ToListAsync(cancellationToken: token);

                var events = compressedEvents.SelectMany(c => c.Data).ToList();

                Console.WriteLine($"Found {events.Count} events for {l} on {d:d}");

                if (events.Count == 0)
                {
                    return;
                }

                var dir = Path.Combine(basePath, l);
                Directory.CreateDirectory(dir);

                var fileName = $"{l} - {d:yyyy-MM-dd}.{fileFormat}";
                var filePath = Path.Combine(dir, fileName);

                switch (fileFormat)
                {
                    case "csv":
                        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                        await using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            await writer.WriteLineAsync("Location,Timestamp,Event,Param");
                            foreach (var e in events)
                            {
                                var formattedDate = e.Timestamp.ToString(tsFormat, CultureInfo.InvariantCulture);
                                await writer.WriteLineAsync($"{e.LocationIdentifier},{formattedDate},{e.EventCode},{e.EventParam}");
                            }
                        }
                        break;

                    case "json":
                        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                        {
                            await JsonSerializer.SerializeAsync(fileStream, events, jsonOptions, token);
                        }
                        break;
                }
            });
        }
    }
}