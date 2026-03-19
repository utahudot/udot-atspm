#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeSpaceDiagram/TimeSpaceDiagramSrmCsvSource.cs
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

using System.Globalization;
using System.IO.Compression;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.Atspm.Business.TimeSpaceDiagram
{
    public class SrmEntityTrackPoint
    {
        public string Time { get; set; } = string.Empty;
        public double Distance { get; set; }
        public long TimestampMs { get; set; }
    }

    public class SrmEntityTrack
    {
        public string EntityId { get; set; } = "unknown";
        public List<SrmEntityTrackPoint> Points { get; set; } = new();
        public string StartingIntersection { get; set; } = string.Empty;
        public DirectionTypes HeadingDirection { get; set; } = DirectionTypes.NA;
    }

    internal class ParsedSrmRow
    {
        public string EntityId { get; set; } = "unknown";
        public string IntersectionId { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lon { get; set; }
        public long TimestampMs { get; set; }
        public double? HeadingDegrees { get; set; }
        public DirectionTypes HeadingDirection { get; set; } = DirectionTypes.NA;
    }

    /// <summary>
    /// Temporary SRM provider backed by a local CSV file.
    /// Future implementation can replace this with an API-backed source.
    /// </summary>
    public class TimeSpaceDiagramSrmCsvSource : ITimeSpaceDiagramSrmSource
    {
        private readonly ILocationRepository locationRepository;

        public TimeSpaceDiagramSrmCsvSource(ILocationRepository locationRepository)
        {
            this.locationRepository = locationRepository;
        }

        public List<SrmEntityTrack> GetTracks(
            DateTime startLocal,
            DateTime endLocal,
            IEnumerable<RouteLocation> routeLocations,
            string? csvContentBase64 = null)
        {
            var routeLocationsList = (routeLocations ?? Enumerable.Empty<RouteLocation>())
                .Where(r => !string.IsNullOrWhiteSpace(r.LocationIdentifier))
                .ToList();

            var allowedIntersectionIds = new HashSet<string>(
                routeLocationsList
                    .Select(r => r.LocationIdentifier?.Trim())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Cast<string>(),
                StringComparer.OrdinalIgnoreCase);

            if (allowedIntersectionIds.Count == 0)
            {
                return new List<SrmEntityTrack>();
            }

            var routeLocationCoordinates = GetRouteLocationCoordinates(
                routeLocationsList,
                startLocal);
            var rows = ParseRows(
                    startLocal,
                    endLocal,
                    allowedIntersectionIds,
                    csvContentBase64)
                .OrderBy(r => r.TimestampMs)
                .ToList();
            if (!rows.Any())
            {
                return new List<SrmEntityTrack>();
            }

            var grouped = rows
                .GroupBy(r => string.IsNullOrWhiteSpace(r.EntityId) ? "unknown" : r.EntityId)
                .ToList();

            var tracks = new List<SrmEntityTrack>();
            foreach (var group in grouped)
            {
                var ordered = group.OrderBy(r => r.TimestampMs).ToList();
                var anchorCoordinates = GetAnchorCoordinatesForEntity(
                    ordered,
                    routeLocationCoordinates);
                if (anchorCoordinates != null && ordered.Count > 0)
                {
                    var startIndex = FindClosestRowIndex(
                        ordered,
                        anchorCoordinates.Value.Latitude,
                        anchorCoordinates.Value.Longitude);

                    if (startIndex > 0)
                    {
                        ordered = ordered.Skip(startIndex).ToList();
                    }
                }

                var points = new List<SrmEntityTrackPoint>();
                var startingIntersection = ordered
                    .Select(r => r.IntersectionId)
                    .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)) ?? string.Empty;

                double totalDistance = 0;
                double? lastLat = null;
                double? lastLon = null;

                foreach (var row in ordered)
                {
                    if (lastLat.HasValue && lastLon.HasValue)
                    {
                        totalDistance += Haversine(lastLat.Value, lastLon.Value, row.Lat, row.Lon);
                    }

                    points.Add(new SrmEntityTrackPoint
                    {
                        Time = FormatLocalTimestamp(
                            DateTimeOffset.FromUnixTimeMilliseconds(row.TimestampMs).LocalDateTime),
                        Distance = totalDistance,
                        TimestampMs = row.TimestampMs
                    });

                    lastLat = row.Lat;
                    lastLon = row.Lon;
                }

                if (points.Count > 0)
                {
                    tracks.Add(new SrmEntityTrack
                    {
                        EntityId = group.Key,
                        Points = points.Take(500).ToList(),
                        StartingIntersection = startingIntersection,
                        HeadingDirection = GetTrackHeadingDirection(ordered)
                    });
                }
            }

            return tracks
                .OrderBy(t => t.EntityId, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private Dictionary<string, (double Latitude, double Longitude)> GetRouteLocationCoordinates(
            IEnumerable<RouteLocation> routeLocations,
            DateTime startLocal)
        {
            var coordinatesByLocationId = new Dictionary<string, (double Latitude, double Longitude)>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var routeLocation in routeLocations.OrderBy(r => r.Order))
            {
                if (string.IsNullOrWhiteSpace(routeLocation.LocationIdentifier))
                {
                    continue;
                }

                var location = locationRepository.GetLatestVersionOfLocation(
                    routeLocation.LocationIdentifier,
                    startLocal);

                if (location != null)
                {
                    coordinatesByLocationId[routeLocation.LocationIdentifier] =
                        (location.Latitude, location.Longitude);
                }
            }

            return coordinatesByLocationId;
        }

        private static (double Latitude, double Longitude)? GetAnchorCoordinatesForEntity(
            IReadOnlyList<ParsedSrmRow> orderedRows,
            Dictionary<string, (double Latitude, double Longitude)> routeLocationCoordinates)
        {
            if (orderedRows.Count == 0 || routeLocationCoordinates.Count == 0)
            {
                return null;
            }

            // Use the first available SRM intersection id in this entity track
            // that overlaps with routeLocation.locationIdentifier.
            foreach (var row in orderedRows)
            {
                if (!string.IsNullOrWhiteSpace(row.IntersectionId) &&
                    routeLocationCoordinates.TryGetValue(row.IntersectionId, out var coordinates))
                {
                    return coordinates;
                }
            }

            return null;
        }

        private static int FindClosestRowIndex(
            IReadOnlyList<ParsedSrmRow> rows,
            double latitude,
            double longitude)
        {
            var closestIndex = 0;
            var closestDistance = double.MaxValue;

            for (var i = 0; i < rows.Count; i++)
            {
                var currentDistance = Haversine(latitude, longitude, rows[i].Lat, rows[i].Lon);
                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private IEnumerable<ParsedSrmRow> ParseRows(
            DateTime startLocal,
            DateTime endLocal,
            HashSet<string> allowedIntersectionIds,
            string? csvContentBase64)
        {
            if (string.IsNullOrWhiteSpace(csvContentBase64))
            {
                return Array.Empty<ParsedSrmRow>();
            }

            try
            {
                var bytes = Convert.FromBase64String(csvContentBase64);
                using var stream = new MemoryStream(bytes);
                Stream contentStream = stream;
                if (IsGzip(stream))
                {
                    contentStream = new GZipStream(stream, CompressionMode.Decompress);
                }

                using (contentStream)
                {
                using var reader = new StreamReader(contentStream);
                return ParseRowsFromReader(
                    reader,
                    startLocal,
                    endLocal,
                    allowedIntersectionIds);
                }
            }
            catch (FormatException)
            {
                return Array.Empty<ParsedSrmRow>();
            }
            catch (InvalidDataException)
            {
                return Array.Empty<ParsedSrmRow>();
            }
        }

        private static bool IsGzip(Stream stream)
        {
            if (!stream.CanSeek || stream.Length < 2)
            {
                return false;
            }

            var originalPosition = stream.Position;
            var first = stream.ReadByte();
            var second = stream.ReadByte();
            stream.Position = originalPosition;

            return first == 0x1F && second == 0x8B;
        }

        private static IEnumerable<ParsedSrmRow> ParseRowsFromReader(
            StreamReader reader,
            DateTime startLocal,
            DateTime endLocal,
            HashSet<string> allowedIntersectionIds)
        {
            var headerLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return Array.Empty<ParsedSrmRow>();
            }

            var delimiter = DetectDelimiter(headerLine);
            var headers = SplitDelimitedLine(headerLine, delimiter)
                .Select(NormalizeHeader)
                .ToList();

            var entityIdIndex = FindHeaderIndex(headers, "REQUESTORNAME");
            var intersectionIdIndex = FindHeaderIndex(headers, "INTERSECTIONID", "LOCATIONIDENTIFIER", "LOCATIONID");
            var latIndex = FindHeaderIndex(headers, "LATITUDE");
            var lonIndex = FindHeaderIndex(headers, "LON", "LONG");
            var headingIndex = FindHeaderIndex(headers, "HEADING");
            var timestampIndex = FindHeaderIndex(headers, "TMSTPUTC", "DATETIME");
            var dateIndex = FindHeaderIndex(headers, "DATE");
            var timeIndex = FindHeaderIndex(headers, "TIME", "TMSTP");

            if (latIndex == -1 || lonIndex == -1)
            {
                return Array.Empty<ParsedSrmRow>();
            }

            var rows = new List<ParsedSrmRow>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var cols = SplitDelimitedLine(line, delimiter).Select(CleanCell).ToList();
                if (latIndex >= cols.Count || lonIndex >= cols.Count)
                {
                    continue;
                }

                if (!double.TryParse(cols[latIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                    !double.TryParse(cols[latIndex], NumberStyles.Float, CultureInfo.CurrentCulture, out lat))
                {
                    continue;
                }

                if (!double.TryParse(cols[lonIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) &&
                    !double.TryParse(cols[lonIndex], NumberStyles.Float, CultureInfo.CurrentCulture, out lon))
                {
                    continue;
                }

                var rawTimestamp = timestampIndex != -1 && timestampIndex < cols.Count ? cols[timestampIndex] : string.Empty;
                var rawDate = dateIndex != -1 && dateIndex < cols.Count ? cols[dateIndex] : string.Empty;
                var rawTime = timeIndex != -1 && timeIndex < cols.Count ? cols[timeIndex] : string.Empty;

                var parsedUtc = ParseUtcTimestamp(rawTimestamp, rawDate, rawTime);
                if (parsedUtc == null)
                {
                    continue;
                }

                var localTime = parsedUtc.Value.ToLocalTime();
                if (localTime < startLocal || localTime > endLocal)
                {
                    continue;
                }

                var intersectionId = intersectionIdIndex != -1 && intersectionIdIndex < cols.Count
                    ? cols[intersectionIdIndex]
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(intersectionId) ||
                    !allowedIntersectionIds.Contains(intersectionId))
                {
                    continue;
                }
                var headingDegrees = ParseHeadingDegrees(cols, headingIndex);

                rows.Add(new ParsedSrmRow
                {
                    EntityId = entityIdIndex != -1 && entityIdIndex < cols.Count
                        ? cols[entityIdIndex]
                        : "unknown",
                    IntersectionId = intersectionId,
                    Lat = lat,
                    Lon = lon,
                    TimestampMs = new DateTimeOffset(parsedUtc.Value).ToUnixTimeMilliseconds(),
                    HeadingDegrees = headingDegrees,
                    HeadingDirection = GetHeadingDirection(headingDegrees)
                });
            }

            return rows;
        }

        private static DateTime? ParseUtcTimestamp(string timestampValue, string dateValue, string timeValue)
        {
            var fromTimestamp = ParseUtcTimestampValue(timestampValue);
            if (fromTimestamp != null)
            {
                return fromTimestamp;
            }

            if (string.IsNullOrWhiteSpace(dateValue))
            {
                return null;
            }

            var utcDate = ParseUtcDateOnly(dateValue);
            if (utcDate == null)
            {
                return null;
            }

            var parsedTime = ParseTimeParts(timeValue);
            if (parsedTime == null)
            {
                return null;
            }

            return utcDate.Value
                .AddHours(parsedTime.Value.Hours)
                .AddMinutes(parsedTime.Value.Minutes)
                .AddSeconds(parsedTime.Value.Seconds)
                .AddMilliseconds(parsedTime.Value.Milliseconds);
        }

        private static DateTime? ParseUtcTimestampValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var raw = value.Trim();
            var hasTimezone = raw.EndsWith("Z", StringComparison.OrdinalIgnoreCase) ||
                System.Text.RegularExpressions.Regex.IsMatch(raw, @"[+-]\d{2}:?\d{2}$");

            if (hasTimezone &&
                DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var zoned))
            {
                return zoned.Kind == DateTimeKind.Utc ? zoned : zoned.ToUniversalTime();
            }

            var isoLike = raw.Contains('T') ? raw : raw.Replace(' ', 'T');
            var utcCandidate = $"{isoLike}Z";
            if (DateTime.TryParse(utcCandidate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedUtc))
            {
                return parsedUtc.Kind == DateTimeKind.Utc ? parsedUtc : parsedUtc.ToUniversalTime();
            }

            return null;
        }

        private static DateTime? ParseUtcDateOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var raw = value.Trim();
            if (DateTime.TryParseExact(
                raw,
                new[] { "yyyy-M-d", "yyyy-MM-dd", "yyyy/M/d", "yyyy/MM/dd" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var ymd))
            {
                return DateTime.SpecifyKind(new DateTime(ymd.Year, ymd.Month, ymd.Day), DateTimeKind.Utc);
            }

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
            {
                var utc = parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
                return DateTime.SpecifyKind(new DateTime(utc.Year, utc.Month, utc.Day), DateTimeKind.Utc);
            }

            return null;
        }

        private static (int Hours, int Minutes, int Seconds, int Milliseconds)? ParseTimeParts(string value)
        {
            var raw = (value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(raw))
            {
                return (0, 0, 0, 0);
            }

            var tokens = raw.Split(':');
            if (tokens.Length < 2 || tokens.Length > 3)
            {
                return null;
            }

            var last = tokens[^1];
            var secParts = last.Split('.');
            if (!int.TryParse(secParts[0], out var seconds))
            {
                return null;
            }

            var millis = 0;
            if (secParts.Length > 1)
            {
                var fraction = $"0.{secParts[1]}";
                if (!double.TryParse(fraction, NumberStyles.Float, CultureInfo.InvariantCulture, out var fractionalSeconds))
                {
                    return null;
                }

                millis = (int)Math.Floor(fractionalSeconds * 1000);
            }

            var hours = 0;
            var minutes = 0;

            if (tokens.Length == 2)
            {
                if (!int.TryParse(tokens[0], out minutes))
                {
                    return null;
                }
            }
            else
            {
                if (!int.TryParse(tokens[0], out hours) || !int.TryParse(tokens[1], out minutes))
                {
                    return null;
                }
            }

            return (hours, minutes, seconds, millis);
        }

        private static int FindHeaderIndex(IReadOnlyList<string> headers, params string[] tokens)
        {
            for (var i = 0; i < headers.Count; i++)
            {
                if (tokens.Any(t => headers[i].Contains(t, StringComparison.Ordinal)))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string NormalizeHeader(string value)
        {
            return new string(value
                .ToUpperInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
        }

        private static char DetectDelimiter(string headerLine)
        {
            var candidates = new[] { ',', '\t', ';', '|' };
            return candidates
                .OrderByDescending(d => headerLine.Count(c => c == d))
                .First();
        }

        private static string CleanCell(string value)
        {
            var trimmed = (value ?? string.Empty).Trim();
            if (trimmed.Length >= 2 && trimmed[0] == '"' && trimmed[^1] == '"')
            {
                trimmed = trimmed[1..^1];
            }

            return trimmed;
        }

        private static List<string> SplitDelimitedLine(string line, char delimiter)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (ch == delimiter && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            result.Add(current.ToString());
            return result;
        }

        private static string FormatLocalTimestamp(DateTime localDateTime)
        {
            return localDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static double? ParseHeadingDegrees(List<string> cols, int headingIndex)
        {
            if (headingIndex == -1 || headingIndex >= cols.Count)
            {
                return null;
            }

            var raw = cols[headingIndex];
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var headingUnits) &&
                !double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out headingUnits))
            {
                return null;
            }

            // SRM heading is encoded in 0.0125-degree units.
            var degrees = headingUnits * 0.0125;
            degrees %= 360.0;
            if (degrees < 0)
            {
                degrees += 360.0;
            }

            return degrees;
        }

        private static DirectionTypes GetHeadingDirection(double? headingDegrees)
        {
            if (!headingDegrees.HasValue)
            {
                return DirectionTypes.NA;
            }

            var d = headingDegrees.Value;
            if (d >= 337.5 || d < 22.5)
            {
                return DirectionTypes.NB;
            }

            if (d < 67.5)
            {
                return DirectionTypes.NE;
            }

            if (d < 112.5)
            {
                return DirectionTypes.EB;
            }

            if (d < 157.5)
            {
                return DirectionTypes.SE;
            }

            if (d < 202.5)
            {
                return DirectionTypes.SB;
            }

            if (d < 247.5)
            {
                return DirectionTypes.SW;
            }

            if (d < 292.5)
            {
                return DirectionTypes.WB;
            }

            return DirectionTypes.NW;
        }

        private static DirectionTypes GetTrackHeadingDirection(IReadOnlyList<ParsedSrmRow> rows)
        {
            if (rows.Count == 0)
            {
                return DirectionTypes.NA;
            }

            // Use most frequent non-unknown row direction for the entity track.
            var heading = rows
                .Where(r => r.HeadingDirection != DirectionTypes.NA)
                .GroupBy(r => r.HeadingDirection)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            return heading == default ? DirectionTypes.NA : heading;
        }

        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;
            static double ToRad(double d) => d * Math.PI / 180.0;

            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return 2 * R * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)) * 3.28084;
        }
    }
}
