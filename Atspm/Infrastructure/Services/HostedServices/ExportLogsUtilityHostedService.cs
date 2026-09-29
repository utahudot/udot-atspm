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
    public class ExtractEventLogHostedService(ILogger<ExtractEventLogHostedService> log, IServiceScopeFactory serviceProvider, IOptions<EventLogExtractConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly IOptions<EventLogExtractConfiguration> _options = options;

        public override async Task Process(IServiceScope scope, Stopwatch stopwatch = null, CancellationToken cancellationToken = default)
        {
            var eventRepo = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();

            var dates = _options.Value.Dates;
            var tsFormat = _options.Value.DateTimeFormat;
            var path = _options.Value.Path;
            var includedLocations = _options.Value.Included;
            var excludedLocations = _options.Value.Excluded;
            var fileFormat = _options.Value.FileFormat;

            Console.WriteLine($"Exporting event logs to {path} in {fileFormat} format with timestamp format {tsFormat}");

            foreach (var d in dates)
            {
                var locs = eventRepo.GetList().Select(s => s.LocationIdentifier)
                    .Distinct()
                    .AsEnumerable()
                    .Where(w => !(includedLocations?.Count() > 0) || includedLocations.Any(a => w == a))
                    .Where(w => !(excludedLocations?.Count() > 0) || !excludedLocations.Any(a => w == a))
                    .ToList();

                foreach (var l in locs)
                {
                    Console.WriteLine($"Processing {l} for {d.ToShortDateString()}");

                    var compressedEvents = await eventRepo.GetData<IndianaEvent>(l, d, d.AddDays(1).AddTicks(-1)).ToListAsync(cancellationToken: cancellationToken);

                    Console.WriteLine($"Found {compressedEvents.Count} events for {l} on {d.ToShortDateString()}");

                    foreach (var c in compressedEvents)
                    {
                        var dir = new DirectoryInfo(Path.Combine(path, l));

                        if (!dir.Exists)
                        {
                            dir.Create();
                        }

                        switch (fileFormat)
                        {
                            case "csv":

                                var csvBuilder = new StringBuilder();

                                csvBuilder.AppendLine("Location,Timestamp,Event,Param");

                                foreach (var e in c.Data)
                                {
                                    string formattedDate = e.Timestamp.ToString(tsFormat, CultureInfo.InvariantCulture);

                                    csvBuilder.AppendLine($"{e.LocationIdentifier},{formattedDate},{e.EventCode},{e.EventParam}");
                                }

                                await File.WriteAllTextAsync(Path.Combine(dir.FullName, $"{l} - {d:yyyy-MM-dd}.csv"), csvBuilder.ToString(), Encoding.UTF8, cancellationToken);

                                break;
                            case "json":

                                var json = JsonSerializer.Serialize(c.Data, new JsonSerializerOptions { WriteIndented = true });

                                await File.WriteAllTextAsync(Path.Combine(dir.FullName, $"{l} - {d:yyyy-MM-dd}.json"), json, Encoding.UTF8, cancellationToken);

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}