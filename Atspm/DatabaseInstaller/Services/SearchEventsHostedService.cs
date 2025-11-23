using DatabaseInstaller.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace DatabaseInstaller.Services;

public sealed class SearchEventsHostedService : IHostedService
{
    private readonly ILogger<SearchEventsHostedService> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly TransferCommandConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILocationRepository _locationRepository;
    private readonly ConcurrentBag<IndianaEvent> _eventLogs = new();

    public SearchEventsHostedService(
        IOptions<TransferCommandConfiguration> config,
        ILogger<SearchEventsHostedService> logger,
        IHostApplicationLifetime lifetime,
        IServiceScopeFactory scopeFactory, 
        ILocationRepository locationRepository)
    {
        _config = config.Value;
        _logger = logger;
        _lifetime = lifetime;
        _scopeFactory = scopeFactory;
        _locationRepository = locationRepository;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Normalize and validate time bounds
            var start = _config.Start;
            var end = _config.End;
            if (start >= end)
            {
                _logger.LogWarning("Start ({Start}) is not before End ({End}). Nothing to search.", start, end);
                return;
            }

            // Parse optional filters
            var eventCodes = ParseEventCodes(_config.EventCodes);
            var locations = new List<string>();
            if (locations.Count == 0)
            {
                locations = _locationRepository.GetLatestVersionOfAllLocations().Select(l => l.LocationIdentifier)
                    .ToList();
                //_logger.LogWarning("No locations provided. Set Locations in configuration (comma-separated).");
                //return;
            }
            else
            {
                locations = ParseList(_config.Locations);
            }

                _logger.LogInformation("Searching events between {Start} and {End} for {Count} locations{CodesHint}.",
                    start, end, locations.Count, eventCodes.Count > 0 ? $" and {eventCodes.Count} event codes" : string.Empty);

            // Bounded concurrency
            const int maxDegreeOfParallelism = 6;
            using var gate = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = locations.Select(async loc =>
            {
                await gate.WaitAsync(cancellationToken);
                try
                {
                    await ProcessLocationAsync(loc, start, end, eventCodes, cancellationToken);
                }
                finally
                {
                    gate.Release();
                }
            });

            await Task.WhenAll(tasks);

            foreach (var log in _eventLogs
                .OrderBy(e => e.EventCode)
                .ThenBy(e => e.LocationIdentifier)
                .ThenBy(e => e.Timestamp))
            {
                _logger.LogInformation("Code: {EventCode} Time: {Timestamp:o} Location: {LocationId}",
                    log.EventCode, log.Timestamp, log.LocationIdentifier);
            }

            var outFile = string.IsNullOrWhiteSpace(_config.OutputCsvPath)
                ? GetDefaultExportPath(start, end)  // from snippet above
                : _config.OutputCsvPath;

            Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
            await WriteCsvAsync(outFile, _eventLogs, cancellationToken);


            _logger.LogInformation("CSV written: {Path}", outFile);


            _logger.LogInformation("Search complete. Found {Count} matching events.", _eventLogs.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Search canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while searching.");
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private static string GetDefaultExportPath(DateTime start, DateTime end)
    {
        var baseDir = AppContext.BaseDirectory; // always writable
                                                // Or use LocalAppData to avoid CFA:
                                                // var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvenueAtspm", "exports");

        Directory.CreateDirectory(baseDir);
        return Path.Combine(baseDir, $"events_{start:yyyyMMddTHHmmss}_{end:yyyyMMddTHHmmss}.csv");
    }


    private static async Task WriteCsvAsync(
    string path,
    IEnumerable<IndianaEvent> events,
    CancellationToken ct)
    {
        // Order once for stable output
        var ordered = events
            .OrderBy(e => e.EventCode)
            .ThenBy(e => e.LocationIdentifier)
            .ThenBy(e => e.Timestamp);

        // Choose writer (supports gzip if path ends with .gz)
        var isGzip = path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
        await using Stream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using Stream targetStream = isGzip
            ? new GZipStream(fileStream, CompressionLevel.SmallestSize, leaveOpen: false)
            : fileStream;

        await using var writer = new StreamWriter(targetStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1 << 16, leaveOpen: false);

        // Header
        await writer.WriteLineAsync("Timestamp,LocationIdentifier,EventCode,EventParam");

        // Rows
        foreach (var ev in ordered)
        {
            var line = string.Join(",", Csv(ev.Timestamp.ToString("o", CultureInfo.InvariantCulture)),
                                        Csv(ev.LocationIdentifier),
                                        ev.EventCode.ToString(CultureInfo.InvariantCulture),
                                        ev.EventParam.ToString(CultureInfo.InvariantCulture));

            //var line = string.Join(",",
            //    Csv(ev.Timestamp.Kind == DateTimeKind.Utc ? ev.Timestamp.ToString("o", CultureInfo.InvariantCulture)
            //                                              : DateTime.SpecifyKind(ev.Timestamp, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture)),
            //    Csv(ev.LocationIdentifier),
            //    ev.EventCode.ToString(CultureInfo.InvariantCulture));

            await writer.WriteLineAsync(line);
            if (ct.IsCancellationRequested) break;
        }

        await writer.FlushAsync();
    }

    // Minimal CSV escape: wrap in quotes; double any quotes inside.
    private static string Csv(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "\"\"";
        return $"\"{s.Replace("\"", "\"\"")}\"";
    }


    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ProcessLocationAsync(
        string locationIdentifier,
        DateTime startUtc,
        DateTime endUtc,
        IReadOnlyCollection<short> eventCodes,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing location {LocationId}", locationIdentifier);

        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();

        // Compose query as much as the repository supports
        // Prefer a repository API that accepts filters to push to DB.
        var query = repo.GetEventsBetweenDates(locationIdentifier, startUtc, endUtc);

        // If GetEventsBetweenDates returns IQueryable<IndianaEvent>, keep composing:
        if (query is IQueryable<IndianaEvent> iq)
        {
            if (eventCodes.Count > 0)
                iq = iq.Where(e => eventCodes.Contains(e.EventCode));

            // Always use AsNoTracking for read-only scans (if EF-backed)
            iq = iq.AsNoTracking().OrderBy(e => e.Timestamp);

            var results = await iq.ToListAsync(cancellationToken);
            AddResults(locationIdentifier, results);
            return;
        }

        // Otherwise, materialized enumerable: filter in-memory as a fallback
        var materialized = query
            .Where(e => eventCodes.Count == 0 || eventCodes.Contains(e.EventCode))
            .OrderBy(e => e.Timestamp)
            .ToList();

        AddResults(locationIdentifier, materialized);
    }

    private void AddResults(string locationIdentifier, List<IndianaEvent> results)
    {
        if (results.Count > 0)
        {
            foreach (var log in results)
                _eventLogs.Add(log);

            _logger.LogInformation("Found {Count} matching events for location {LocationId}", results.Count, locationIdentifier);
        }
        else
        {
            _logger.LogInformation("No matching events found for location {LocationId}", locationIdentifier);
        }
    }

    private static List<short> ParseEventCodes(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new();
        var list = new List<short>();
        foreach (var token in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (short.TryParse(token, out var code)) list.Add(code);
        return list;
    }

    private static List<string> ParseList(string? csv)
        => string.IsNullOrWhiteSpace(csv)
            ? new()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static DateTime EnsureUtc(DateTime dt)
        => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
