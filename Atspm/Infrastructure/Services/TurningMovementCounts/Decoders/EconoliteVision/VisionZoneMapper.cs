using Utah.Udot.Atspm.Exceptions;
using System.Text.RegularExpressions;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.TurningMovementCounts.Decoders.EconoliteVision;

public static class VisionZoneMapper
{
    // Persisted local timestamps have no kind; JSON camera timestamps are UTC.
    public static DateTime LocalTimestamp(DateTime timestamp, TimeZoneInfo zone, bool stored) =>
        stored && timestamp.Kind == DateTimeKind.Unspecified ? timestamp
            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(timestamp, DateTimeKind.Utc), zone);

    public static IEnumerable<VisionCameraStatisticsEvent> InRequestedRange(
        IEnumerable<VisionCameraStatisticsEvent> rows, TmcDecodeRequest request, TimeZoneInfo zone, bool stored) =>
        rows.Where(row => {
            var local = LocalTimestamp(row.Timestamp, zone, stored);
            return local >= request.Options.Start && local < request.Options.End;
        });

    public static string Setting(IReadOnlyDictionary<string, object> settings, string key, string fallback = "") =>
        settings.TryGetValue(key, out var value) && value != null ? value.ToString() ?? fallback : fallback;

    public static DirectionTypes Direction(string text) => Regex.Replace(text, @"[\s_-]", "").ToUpperInvariant() switch {
        "NB" or "NORTH" or "NORTHBOUND" or "N" => DirectionTypes.NB,
        "SB" or "SOUTH" or "SOUTHBOUND" or "S" => DirectionTypes.SB,
        "EB" or "EAST" or "EASTBOUND" or "E" => DirectionTypes.EB,
        "WB" or "WEST" or "WESTBOUND" or "W" => DirectionTypes.WB,
        "NE" or "NEB" or "NORTHEAST" or "NORTHEASTBOUND" => DirectionTypes.NE,
        "NW" or "NWB" or "NORTHWEST" or "NORTHWESTBOUND" => DirectionTypes.NW,
        "SE" or "SEB" or "SOUTHEAST" or "SOUTHEASTBOUND" => DirectionTypes.SE,
        "SW" or "SWB" or "SOUTHWEST" or "SOUTHWESTBOUND" => DirectionTypes.SW,
        _ => DirectionTypes.NA };

    public static DirectionTypes DirectionFromZoneName(string name)
    {
        // Whole direction tokens, optionally followed by lane/channel numbers.
        // Do not mistake names such as Northgate or Westwood for directions.
        var matches = Regex.Matches(name, @"(?<![a-z])(?:north[\s_-]*(?:east|west)|south[\s_-]*(?:east|west)|north|south|east|west)(?:[\s_-]*bound)?(?![a-z])|(?<![a-z])(?:ne|nw|se|sw|n|s|e|w)b?(?![a-z])", RegexOptions.IgnoreCase);
        var directions = matches.Cast<Match>().Select(m => Direction(m.Value)).Distinct().ToArray();
        return directions.Length == 1 ? directions[0] : DirectionTypes.NA;
    }

    public static TmcDecodeResult Map(TmcDecodeRequest request, IEnumerable<VisionCameraStatisticsEvent> events,
        int interval, TimeZoneInfo zone, bool stored)
    {
        var warnings = new List<string>();
        var rows = events
            .GroupBy(e => (e.ZoneName, e.Timestamp)).Select(g => g.Last()).ToList();
        var layer = Setting(request.Settings, "TmcLayer", "StopBar");
        if (!new[] { "StopBar", "Advance", "RedLightRunning" }.Contains(layer))
            throw new ReportException(400, $"Device {request.Device.Id}: invalid TmcLayer {layer}.");
        var resolved = new List<(VisionCameraStatisticsEvent Row, DirectionTypes Direction, string Label, LaneTypes Lane, int? Number, bool Explicit, int Setback)>();
        foreach (var group in rows.GroupBy(e => e.ZoneName ?? ""))
        {
            var name = group.Key;
            var code = name.Split('-')[0];
            var match = Regex.Match(code, @"^(?<type>RR|A|[NSEWLRBP])(?<phase>[1-8])(?<digits>\d*)$", RegexOptions.IgnoreCase);
            var type = match.Groups["type"].Value.ToUpperInvariant();
            var digits = match.Groups["digits"].Value;
            var zoneLayer = type == "A" ? "Advance" : type == "RR" ? "RedLightRunning" : "StopBar";
            if (zoneLayer != layer && type != "P")
            {
                warnings.Add($"Camera {request.Device.DeviceIdentifier}: ignored {name} ({group.Sum(r => r.Volume)} vehicles, {zoneLayer} layer).");
                continue;
            }
            var direction = DirectionFromZoneName(name);
            if (direction == DirectionTypes.NA && type is "N" or "S" or "E" or "W") direction = Direction(type);
            var label = direction == DirectionTypes.NA ? (string.IsNullOrWhiteSpace(name) ? $"Unnamed zone {group.First().ZoneId}" : name) : null;
            var laneType = type == "P" ? LaneTypes.Ped : type == "B" ? LaneTypes.Bike : LaneTypes.V;
            int? number = null;
            var setback = type == "A" && digits.Length >= 2 ? digits[0] - '0' : 0;
            foreach (var row in group) resolved.Add((row, direction, label, laneType, number, number.HasValue, setback));
        }
        var closest = resolved.Where(r => r.Setback > 0).GroupBy(r => (r.Direction, Phase: r.Direction == DirectionTypes.NA ? Regex.Match(r.Row.ZoneName, @"^A([1-8])", RegexOptions.IgnoreCase).Groups[1].Value : ""))
            .ToDictionary(g => g.Key, g => g.Min(r => r.Setback));
        var counts = new List<MovementCount>();
        foreach (var item in resolved)
        {
            if (item.Setback > 0 && item.Setback != closest[(item.Direction, item.Direction == DirectionTypes.NA ? Regex.Match(item.Row.ZoneName, @"^A([1-8])", RegexOptions.IgnoreCase).Groups[1].Value : "")])
            {
                warnings.Add($"Ignored advance zone {item.Row.ZoneName}: using the closest setback.");
                continue;
            }
            var timestamp = LocalTimestamp(item.Row.Timestamp, zone, stored);
            // Choose the layer and closest setback from the returned inventory before
            // filtering time; sparse nearby bins must not promote a farther detector.
            if (timestamp < request.Options.Start || timestamp >= request.Options.End) continue;
            var movements = item.Lane == LaneTypes.Ped
                ? new[] { (MovementTypes.T, item.Row.LeftToRightCount + item.Row.RightToLeftCount) }
                : new[] { (MovementTypes.T, item.Row.ThroughCount), (MovementTypes.L, item.Row.LeftTurnCount), (MovementTypes.R, item.Row.RightTurnCount) };
            foreach (var (movement, volume) in movements)
            {
                if (volume < 0) throw new ReportException(502, $"Camera {request.Device.DeviceIdentifier}: negative movement count.");
                counts.Add(new MovementCount(item.Direction, movement, item.Lane, item.Number, timestamp, interval, volume) {
                    ZoneName = item.Row.ZoneName, DirectionLabel = item.Label, DeviceId = request.Device.Id,
                    ExplicitLane = item.Explicit, AdvanceSetback = item.Setback });
            }
        }
        return new TmcDecodeResult(counts, warnings.Distinct().ToArray());
    }
}
