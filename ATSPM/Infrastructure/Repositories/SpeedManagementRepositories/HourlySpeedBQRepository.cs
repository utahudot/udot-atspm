using ATSPM.Application.Business.RouteSpeed;
using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Domain.Extensions;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementRepositories
{
    ///<inheritdoc cref="IHourlySpeedRepository"/>
    public class HourlySpeedBQRepository : ATSPMRepositoryBQBase<HourlySpeed>, IHourlySpeedRepository
    {

        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<HourlySpeed>> _logger;

        public HourlySpeedBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<HourlySpeed>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
        }


        public async Task AddHourlySpeedAsync(HourlySpeed hourlySpeed)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            var insertRow = CreateRow(hourlySpeed);
            await table.InsertRowAsync(insertRow);
        }

        public async Task AddHourlySpeedsAsync(List<HourlySpeed> hourlySpeeds)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            List<BigQueryInsertRow> insertRows = new List<BigQueryInsertRow>();
            int batchSize = 10000;

            foreach (var hourlySpeed in hourlySpeeds)
            {
                insertRows.Add(CreateRow(hourlySpeed));

                if (insertRows.Count == batchSize)
                {
                    await table.InsertRowsAsync(insertRows);
                    insertRows.Clear(); // Clear the list for the next batch
                }
            }

            // Insert any remaining rows that didn't make a full batch
            if (insertRows.Count > 0)
            {
                await table.InsertRowsAsync(insertRows);
            }
        }

        #region Overrides
        protected override BigQueryInsertRow CreateRow(HourlySpeed item)
        {
            return new BigQueryInsertRow
            {
                { "Date", item.Date.AsBigQueryDate() },
                {"BinStartTime", item.BinStartTime.TimeOfDay },
                {"RouteId", item.RouteId },
                {"SourceId", item.SourceId },
                {"ConfidenceId", item.ConfidenceId },
                {"Average", item.Average },
                {"FifteenthSpeed", item.FifteenthSpeed },
                {"EightyFifthSpeed", item.EightyFifthSpeed },
                {"NinetyFifthSpeed", item.NinetyFifthSpeed },
                {"NinetyNinthSpeed", item.NinetyNinthSpeed },
                {"Violation", item.Violation },
                {"Flow", item.Flow }
            };
        }

        protected override HourlySpeed MapRowToEntity(BigQueryRow row)
        {
            return new HourlySpeed
            {
                Date = row.GetPropertyValue<DateTime>("Id"),
                BinStartTime = row.GetPropertyValue<DateTime>("BinStartTime"),
                RouteId = row.GetPropertyValue<int>("RouteId"),
                SourceId = row.GetPropertyValue<int>("SourceId"),
                ConfidenceId = row.GetPropertyValue<int>("ConfidenceId"),
                Average = row.GetPropertyValue<int>("Average"),
                FifteenthSpeed = row.GetPropertyValue<int?>("FifteenthSpeed"),
                EightyFifthSpeed = row.GetPropertyValue<int?>("EightyFifthSpeed"),
                NinetyFifthSpeed = row.GetPropertyValue<int?>("NinetyFifthSpeed"),
                NinetyNinthSpeed = row.GetPropertyValue<int?>("NinetyNinthSpeed"),
                Violation = row.GetPropertyValue<int?>("Violation"),
                Flow = row.GetPropertyValue<int?>("Flow")
            };
        }

        public async Task<List<MonthlyAverage>> GetMonthlyAveragesAsync(int routeId, DateOnly startDate, DateOnly endDate, string daysOfWeek, int sourceId)
        {
            string query = $@"
            SELECT 
                DATE_TRUNC(date, MONTH) AS Month, 
                AVG(Average) AS Average,
                AVG(FifteenthSpeed) AS FifteenthSpeed,
                AVG(EightyFifthSpeed) AS EightyFifthSpeed,
                AVG(NinetyFifthSpeed) AS NinetyFifthSpeed,
                AVG(NinetyNinthSpeed) AS NinetyNinthSpeed,
                AVG(Violation) AS Violation,
                AVG(Flow) AS Flow
            FROM `atspm-406601.speed_dataset.hourly_speed`
            WHERE 
                RouteId = @routeId AND
                SourceId = @sourceId AND
                date BETWEEN @startDate AND @endDate AND
                EXTRACT(DAYOFWEEK FROM date) IN ({daysOfWeek})
            GROUP BY Month
            ORDER BY Month ASC;";

            var parameters = new[]
            {
            new BigQueryParameter("routeId", BigQueryDbType.Int64, routeId),
            new BigQueryParameter("startDate", BigQueryDbType.Date, startDate.ToDateTime(new TimeOnly(0,0))),
            new BigQueryParameter("endDate", BigQueryDbType.Date, endDate.ToDateTime(new TimeOnly(0,0))),
            new BigQueryParameter("sourceId", BigQueryDbType.Int64, sourceId)
        };

            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            return queryResults.Select(row => new MonthlyAverage
            {
                Month = DateTime.Parse(row["Month"].ToString()),
                Average = Convert.ToDouble(row["Average"]),
                FifteenthSpeed = Convert.ToDouble(row["FifteenthSpeed"]),
                EightyFifthSpeed = Convert.ToDouble(row["EightyFifthSpeed"]),
                NinetyFifthSpeed = Convert.ToDouble(row["NinetyFifthSpeed"]),
                NinetyNinthSpeed = Convert.ToDouble(row["NinetyNinthSpeed"]),
                Violation = Convert.ToDouble(row["Violation"]),
                Flow = Convert.ToDouble(row["Flow"])
            }).ToList();
        }

        public async Task<List<DailyAverage>> GetDailyAveragesAsync(int routeId, DateOnly startDate, DateOnly endDate, string daysOfWeek)
        {
            string query = $@"
            SELECT 
                DATE_TRUNC(date, DAY) AS Date, 
                AVG(Average) AS Average,
                AVG(FifteenthSpeed) AS FifteenthSpeed,
                AVG(EightyFifthSpeed) AS EightyFifthSpeed,
                AVG(NinetyFifthSpeed) AS NinetyFifthSpeed,
                AVG(NinetyNinthSpeed) AS NinetyNinthSpeed,
                AVG(Violation) AS Violation,
                AVG(Flow) AS Flow
            FROM `atspm-406601.speed_dataset.hourly_speed`
            WHERE 
                RouteId = @routeId AND
                date BETWEEN @startDate AND @endDate AND
                EXTRACT(DAYOFWEEK FROM date) IN ({daysOfWeek})
            GROUP BY Date
            ORDER BY Date ASC;";

            var parameters = new[]
            {
            new BigQueryParameter("routeId", BigQueryDbType.Int64, routeId),
            new BigQueryParameter("startDate", BigQueryDbType.Date, startDate.ToDateTime(new TimeOnly(0,0))),
            new BigQueryParameter("endDate", BigQueryDbType.Date, endDate.ToDateTime(new TimeOnly(0, 0))),
        };

            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            return queryResults.Select(row => new DailyAverage
            {
                Date = DateTime.Parse(row["Date"].ToString()),
                Average = Convert.ToDouble(row["Average"]),
                FifteenthSpeed = Convert.ToDouble(row["FifteenthSpeed"]),
                EightyFifthSpeed = Convert.ToDouble(row["EightyFifthSpeed"]),
                NinetyFifthSpeed = Convert.ToDouble(row["NinetyFifthSpeed"]),
                NinetyNinthSpeed = Convert.ToDouble(row["NinetyNinthSpeed"]),
                Violation = Convert.ToDouble(row["Violation"]),
                Flow = Convert.ToDouble(row["Flow"])
            }).ToList();
        }

        public async Task<List<HourlySpeed>> GetHourlySpeeds(CongestionTrackingOptions options)
        {
            var daysOfWeek = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

            string query = $@"
            SELECT *
            FROM `atspm-406601.speed_dataset.hourly_speed`
            WHERE 
                RouteId = @routeId AND
                date BETWEEN @startDate AND @endDate 
            ORDER BY Date ASC, BinStartTime ASC;";

            var parameters = new[]
            {
            new BigQueryParameter("routeId", BigQueryDbType.Int64, options.SegmentId),
            new BigQueryParameter("startDate", BigQueryDbType.Date, options.StartDate.ToDateTime(new TimeOnly(0,0))),
            new BigQueryParameter("endDate", BigQueryDbType.Date, options.EndDate.ToDateTime(new TimeOnly(0, 0))),
        };

            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            return queryResults.Select(row =>
            {
                var date = DateOnly.Parse(row["Date"].ToString());
                var time = TimeOnly.Parse(row["BinStartTime"].ToString());

                return new HourlySpeed
                {
                    Date = date.ToDateTime(new TimeOnly(0, 0)),
                    BinStartTime = date.ToDateTime(time),
                    Average = Convert.ToInt32(row["Average"]),
                    FifteenthSpeed = Convert.ToInt32(row["FifteenthSpeed"]),
                    EightyFifthSpeed = Convert.ToInt32(row["EightyFifthSpeed"]),
                    NinetyFifthSpeed = Convert.ToInt32(row["NinetyFifthSpeed"]),
                    NinetyNinthSpeed = Convert.ToInt32(row["NinetyNinthSpeed"]),
                    Violation = Convert.ToInt32(row["Violation"]),
                    Flow = Convert.ToInt32(row["Flow"])
                };
            }).ToList();
        }

        public async Task<List<RouteSpeed>> GetRoutesSpeeds(RouteSpeedOptions options)
        {
            var convertedStartDate = DateOnly.FromDateTime(options.StartDate);
            var convertedEndDate = DateOnly.FromDateTime(options.EndDate);
            var convertedStartTime = TimeOnly.FromDateTime(options.StartTime);
            var convertedEndTime = TimeOnly.FromDateTime(options.EndTime);

            try
            {
                var startDateTime = new DateTime(convertedStartDate.Year, convertedStartDate.Month, convertedStartDate.Day, convertedStartTime.Hour, convertedStartTime.Minute, convertedStartTime.Second);
                var endDateTime = new DateTime(convertedEndDate.Year, convertedEndDate.Month, convertedEndDate.Day, convertedEndTime.Hour, convertedEndTime.Minute, convertedEndTime.Second);
                var query = "";
                BigQueryResults queryResults;
                BigQueryParameter[] parameters;
                var daysOfWeekCondition = string.Join(", ", options.DaysOfWeek.Select(day => ((int)day + 1).ToString()));
                switch (options.AnalysisPeriod)
                {
                    //case AnalysisPeriod.AllDay:
                    //    var startDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startTime.Hour, startTime.Minute, startTime.Second);
                    //    var endDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, endTime.Hour, endTime.Minute, endTime.Second);

                    //    query
                    //        .Where(record =>
                    //           record.Start.Date.ToDateTime(new TimeOnly(0, 0)) + record.BinStartTime.ToTimeSpan() >= startDateTime &&
                    //           record.Date.ToDateTime(new TimeOnly(0, 0)) + record.BinStartTime.ToTimeSpan() <= endDateTime);
                    //    break;
                    case AnalysisPeriod.PeekPeriod:
                        query = $@"
                            SELECT *
                            FROM `atspm-406601.speed_dataset.hourly_speed`
                            WHERE 
                                DATE BETWEEN @startDate AND @endDate AND
                                (
                                    (TIME(BinStartTime) BETWEEN TIME '06:00:00' AND TIME '09:00:00') OR
                                    (TIME(BinStartTime) BETWEEN TIME '15:00:00' AND TIME '18:00:00')
                                ) AND
                                EXTRACT(DAYOFWEEK FROM DATE) IN ({daysOfWeekCondition})";

                        parameters = new[]
                        {
                            new BigQueryParameter("startDate", BigQueryDbType.Date, convertedStartDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("endDate", BigQueryDbType.Date, convertedEndDate.ToDateTime(new TimeOnly(0, 0)))
                        };

                        queryResults = await _client.ExecuteQueryAsync(query, parameters);
                        break;
                    default:
                        query = $@"
                                WITH RouteStats AS (
                                    SELECT
                                        hs.RouteId,
                                        AVG(hs.Average) AS Avg,
                                        APPROX_QUANTILES(hs.FifteenthSpeed, 100)[ORDINAL(85)] AS Percentilespd_15,
                                        APPROX_QUANTILES(hs.EightyFifthSpeed, 100)[ORDINAL(85)] AS Percentilespd_85,
                                        APPROX_QUANTILES(hs.NinetyFifthSpeed, 100)[ORDINAL(85)] AS Percentilespd_95,
                                        SUM(hs.Flow) AS Flow
                                    FROM
                                        `atspm-406601.speed_dataset.hourly_speed` AS hs
                                    WHERE
                                        DATE BETWEEN @startDate AND @endDate
                                        AND TIME(hs.BinStartTime) BETWEEN @startTime AND @endTime
                                        AND EXTRACT(DAYOFWEEK FROM DATE) IN (1,2,3,4,5,6,7)
                                    GROUP BY
                                        hs.RouteId
                                )
                                SELECT
                                    rs.RouteId,
                                    rs.Avg,
                                    rs.Percentilespd_15,
                                    rs.Percentilespd_85,
                                    rs.Percentilespd_95,
                                    rs.Flow,
                                    r.SpeedLimit,
                                    ANY_VALUE(ST_AsText(r.Shape)) AS Shape,
                                    IFNULL(
                                        SAFE_CAST(
                                            ROUND(SUM
                                            (CASE 
                                              WHEN rs.Percentilespd_15 >= r.SpeedLimit THEN 0.85 * rs.Flow
                                              WHEN rs.Avg >= r.SpeedLimit THEN 0.5 * rs.Flow
                                              WHEN rs.Percentilespd_85 >= r.SpeedLimit THEN 0.15 * rs.Flow
                                              WHEN rs.Percentilespd_95 >= r.SpeedLimit THEN 0.05 * rs.Flow
                                              ELSE 0
                                              END) / NULLIF(SUM(rs.Flow), 0)) AS INT64), 0) AS EstimatedViolations
                                FROM
                                    RouteStats AS rs
                                JOIN
                                    `atspm-406601.speed_dataset.segment` AS r
                                ON
                                    rs.RouteId = r.Id
                                GROUP BY
                                    rs.RouteId, rs.Avg, rs.Percentilespd_15, rs.Percentilespd_85, rs.Percentilespd_95, rs.Flow, r.SpeedLimit";
                        parameters = new[]
                        {
                            new BigQueryParameter("startDate", BigQueryDbType.Date, convertedStartDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("endDate", BigQueryDbType.Date, convertedEndDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("startTime", BigQueryDbType.Time, convertedStartTime.ToTimeSpan()),
                            new BigQueryParameter("endTime", BigQueryDbType.Time, convertedEndTime.ToTimeSpan())
                        };
                        queryResults = await _client.ExecuteQueryAsync(query, parameters);
                        break;
                }

                List<RouteSpeed> results = new List<RouteSpeed>();
                foreach (BigQueryRow row in queryResults)
                {
                    var reader = new WKTReader();
                    // Access row data as needed
                    var routeId = row["RouteId"];
                    var avg = row["Avg"];
                    var percentile15 = row["Percentilespd_15"];
                    var percentile85 = row["Percentilespd_85"];
                    var percentile95 = row["Percentilespd_95"];
                    var flow = row["Flow"];
                    var estimatedViolations = row["EstimatedViolations"];
                    var speedLimit = row["SpeedLimit"];
                    var wkt = (string)row["Shape"];
                    // Further processing...
                    Geometry shape = wkt != null ? reader.Read(wkt) : null;

                    var result = new RouteSpeed
                    {
                        RouteId = routeId != null ? routeId.ToString() : "",
                        Name = routeId != null ? routeId.ToString() : "",
                        Avg = avg != null ? Math.Round((double)avg, 2) : null,
                        Percentilespd_15 = percentile15 != null ? (long)percentile15 : null,
                        Percentilespd_85 = percentile85 != null ? (long)percentile85 : null,
                        Percentilespd_95 = percentile95 != null ? (long)percentile95 : null,
                        Flow = flow != null ? (long)flow : null,
                        EstimatedViolations = estimatedViolations != null ? (long)estimatedViolations : null,
                        SpeedLimit = speedLimit != null ? (long)speedLimit : 0,
                        Shape = shape,
                    };
                    results.Add(result);

                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static long? GetPercentileSpeed_15(IGrouping<string, BigQueryRow> g)
        {
            var speedList = g
            .Select(row => row["FifteenthSpeed"])
            .Where(speed => speed != null && speed is long)
            .Select(speed => (long)speed)
            .ToList();

            return speedList.Count > 0 ? (int?)Math.Round(speedList.Average()) : null;
        }

        private static long? GetPercentileSpeed_85(IGrouping<string, BigQueryRow> g)
        {
            // Safely retrieve EightyFifthSpeed and ensure it's an integer
            var speedList = g
                .Select(row => row["EightyFifthSpeed"])
                .Where(speed => speed != null && speed is long)
                .Select(speed => (long)speed)
                .ToList();

            return speedList.Count > 0 ? (long?)Math.Round(speedList.Average()) : null;
        }

        private static long? GetPercentileSpeed_95(IGrouping<string, BigQueryRow> g)
        {
            // Safely retrieve NinetyFifthSpeed and ensure it's an integer
            var speedList = g
                .Select(row => row["NinetyFifthSpeed"])
                .Where(speed => speed != null && speed is long)
                .Select(speed => (long)speed)
                .ToList();

            return speedList.Count > 0 ? (long?)Math.Round(speedList.Average()) : null;
        }

        private bool HigherThanViolationThreshold(int violationThreshold, long speed, int speedLimit)
        {
            var test = (speed - speedLimit) / (double)speedLimit * 100;
            return speed > speedLimit && (speed - speedLimit) / (double)speedLimit * 100 >= violationThreshold;
        }

        public override async Task RemoveAsync(HourlySpeed key)
        {

            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key"; // Assuming primary key column is named "Id"
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, key) // Adjust BigQueryDbType as needed
             };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override IQueryable<HourlySpeed> GetList()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = _client.ExecuteQuery(query, parameters).ToList();

            // Map the result to a list of ImpactType objects
            return result.Select(row => MapRowToEntity(row)).ToList().AsQueryable();
        }

        public override HourlySpeed Lookup(object key)
        {
            if (key == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key)
                };
            var results = _client.ExecuteQuery(query, parameters);
            Task<HourlySpeed> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override HourlySpeed Lookup(HourlySpeed item)
        {
            if (item == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item)
                };

            var results = _client.ExecuteQuery(query, parameters);
            Task<HourlySpeed> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override async Task<HourlySpeed> LookupAsync(object key)
        {
            if (key == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key)
                };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override async Task<HourlySpeed> LookupAsync(HourlySpeed item)
        {
            if (item == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item)
                };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override void Remove(HourlySpeed item)
        {
            if (item == null) return;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item)
             };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<HourlySpeed> items)
        {
            var ids = string.Join(", ", items);
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>();

            _client.ExecuteQuery(query, parameters);
        }

        public override async Task RemoveRangeAsync(IEnumerable<HourlySpeed> items)
        {
            var ids = string.Join(", ", items);
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>();

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override async void Update(HourlySpeed item)
        {
            var oldRow = await LookupAsync(new { item.Date, item.BinStartTime, item.RouteId });
            if (oldRow != null)
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");

                queryBuilder.Append($"SourceId = {item.SourceId}, ");
                queryBuilder.Append($"ConfidenceId = {item.ConfidenceId}, ");
                queryBuilder.Append($"Average = {item.Average}, ");

                if (item.FifteenthSpeed.HasValue)
                {
                    queryBuilder.Append($"FifteenthSpeed = {item.FifteenthSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"FifteenthSpeed = NULL, ");
                }

                if (item.EightyFifthSpeed.HasValue)
                {
                    queryBuilder.Append($"EightyFifthSpeed = {item.EightyFifthSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"EightyFifthSpeed = NULL, ");
                }

                if (item.NinetyFifthSpeed.HasValue)
                {
                    queryBuilder.Append($"NinetyFifthSpeed = {item.NinetyFifthSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"NinetyFifthSpeed = NULL, ");
                }

                if (item.NinetyNinthSpeed.HasValue)
                {
                    queryBuilder.Append($"NinetyNinthSpeed = {item.NinetyNinthSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"NinetyNinthSpeed = NULL, ");
                }

                if (item.Violation.HasValue)
                {
                    queryBuilder.Append($"Violation = {item.Violation}, ");
                }
                else
                {
                    queryBuilder.Append($"Violation = NULL, ");
                }

                if (item.Flow.HasValue)
                {
                    queryBuilder.Append($"Flow = {item.Flow}, ");
                }
                else
                {
                    queryBuilder.Append($"Flow = NULL, ");
                }

                // Remove the last comma and space
                queryBuilder.Length -= 2;

                queryBuilder.Append($" WHERE Date = @Date AND BinStartTime = @BinStartTime AND RouteId = @RouteId");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@Date", BigQueryDbType.DateTime, item.Date),
            new BigQueryParameter("@BinStartTime", BigQueryDbType.DateTime, item.BinStartTime),
            new BigQueryParameter("@RouteId", BigQueryDbType.Int64, item.RouteId)
        };

                _client.ExecuteQuery(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Date, BinStartTime, RouteId, SourceId, ConfidenceId, Average, FifteenthSpeed, EightyFifthSpeed, NinetyFifthSpeed, NinetyNinthSpeed, Violation, Flow) " +
                    $"VALUES (" +
                    $"'{item.Date:O}', " +
                    $"'{item.BinStartTime:O}', " +
                    $"{item.RouteId}, " +
                    $"{item.SourceId}, " +
                    $"{item.ConfidenceId}, " +
                    $"{item.Average}, " +
                    $"{(item.FifteenthSpeed.HasValue ? item.FifteenthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.EightyFifthSpeed.HasValue ? item.EightyFifthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.NinetyFifthSpeed.HasValue ? item.NinetyFifthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.NinetyNinthSpeed.HasValue ? item.NinetyNinthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.Violation.HasValue ? item.Violation.Value.ToString() : "NULL")}, " +
                    $"{(item.Flow.HasValue ? item.Flow.Value.ToString() : "NULL")})";

                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQuery(query, parameters);
            }
        }

        public override async Task UpdateAsync(HourlySpeed item)
        {
            var oldRow = await LookupAsync(new { item.Date, item.BinStartTime, item.RouteId });
            if (oldRow != null)
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");

                queryBuilder.Append($"SourceId = {item.SourceId}, ");
                queryBuilder.Append($"ConfidenceId = {item.ConfidenceId}, ");
                queryBuilder.Append($"Average = {item.Average}, ");

                if (item.FifteenthSpeed.HasValue)
                {
                    queryBuilder.Append($"FifteenthSpeed = {item.FifteenthSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"FifteenthSpeed = NULL, ");
                }

                if (item.EightyFifthSpeed.HasValue)
                {
                    queryBuilder.Append($"EightyFifthSpeed = {item.EightyFifthSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"EightyFifthSpeed = NULL, ");
                }

                if (item.NinetyFifthSpeed.HasValue)
                {
                    queryBuilder.Append($"NinetyFifthSpeed = {item.NinetyFifthSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"NinetyFifthSpeed = NULL, ");
                }

                if (item.NinetyNinthSpeed.HasValue)
                {
                    queryBuilder.Append($"NinetyNinthSpeed = {item.NinetyNinthSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"NinetyNinthSpeed = NULL, ");
                }

                if (item.Violation.HasValue)
                {
                    queryBuilder.Append($"Violation = {item.Violation}, ");
                }
                else
                {
                    queryBuilder.Append($"Violation = NULL, ");
                }

                if (item.Flow.HasValue)
                {
                    queryBuilder.Append($"Flow = {item.Flow}, ");
                }
                else
                {
                    queryBuilder.Append($"Flow = NULL, ");
                }

                // Remove the last comma and space
                queryBuilder.Length -= 2;

                queryBuilder.Append($" WHERE Date = @Date AND BinStartTime = @BinStartTime AND RouteId = @RouteId");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@Date", BigQueryDbType.DateTime, item.Date),
            new BigQueryParameter("@BinStartTime", BigQueryDbType.DateTime, item.BinStartTime),
            new BigQueryParameter("@RouteId", BigQueryDbType.Int64, item.RouteId)
        };

                _client.ExecuteQueryAsync(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Date, BinStartTime, RouteId, SourceId, ConfidenceId, Average, FifteenthSpeed, EightyFifthSpeed, NinetyFifthSpeed, NinetyNinthSpeed, Violation, Flow) " +
                    $"VALUES (" +
                    $"'{item.Date:O}', " +
                    $"'{item.BinStartTime:O}', " +
                    $"{item.RouteId}, " +
                    $"{item.SourceId}, " +
                    $"{item.ConfidenceId}, " +
                    $"{item.Average}, " +
                    $"{(item.FifteenthSpeed.HasValue ? item.FifteenthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.EightyFifthSpeed.HasValue ? item.EightyFifthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.NinetyFifthSpeed.HasValue ? item.NinetyFifthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.NinetyNinthSpeed.HasValue ? item.NinetyNinthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.Violation.HasValue ? item.Violation.Value.ToString() : "NULL")}, " +
                    $"{(item.Flow.HasValue ? item.Flow.Value.ToString() : "NULL")})";

                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQueryAsync(query, parameters);
            }
        }

        public override void UpdateRange(IEnumerable<HourlySpeed> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public override async Task UpdateRangeAsync(IEnumerable<HourlySpeed> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }



        #endregion

        #region IApproachRepository

        #endregion
    }
}