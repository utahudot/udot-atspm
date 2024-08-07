using ATSPM.Application.Business.RouteSpeed;
using ATSPM.Application.Repositories.SpeedManagementRepositories;
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

        public async Task<List<HourlySpeed>> GetHourlySpeedsForTimePeriod(Guid segmentId, DateTime startDate, DateTime endDate, DateTime startTime, DateTime endTime)
        {
            string query = $@"
                SELECT *
                FROM `{_datasetId}.{_tableId}`
                WHERE 
                    SegmentId = @segmentId AND
                    Date BETWEEN @startDate AND @endDate AND
                    BinStartTime BETWEEN @startTime AND @endTime
                ORDER BY Date ASC, BinStartTime ASC;";

            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId),
                new BigQueryParameter("startDate", BigQueryDbType.Date, startDate.Date),
                new BigQueryParameter("endDate", BigQueryDbType.Date, endDate.Date),
                new BigQueryParameter("startTime", BigQueryDbType.Time, startTime.TimeOfDay),
                new BigQueryParameter("endTime", BigQueryDbType.Time, endTime.TimeOfDay)
            };

            var result = await _client.ExecuteQueryAsync(query, parameters);
            var monthlyAggregations = new List<HourlySpeed>();
            foreach (var row in result)
            {
                monthlyAggregations.Add(MapRowToEntity(row));
            }

            return monthlyAggregations;
        }

        #region Overrides
        protected override BigQueryInsertRow CreateRow(HourlySpeed item)
        {
            return new BigQueryInsertRow
            {
                { "Date", item.Date.AsBigQueryDate() },
                {"BinStartTime", item.BinStartTime.TimeOfDay },
                {"SegmentId", item.SegmentId },
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
                SegmentId = row.GetPropertyValue<string>("SegmentId"),
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

        public async Task<List<MonthlyAverage>> GetMonthlyAveragesAsync(Guid segmentId, DateOnly startDate, DateOnly endDate, string daysOfWeek, int sourceId)
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
                SegmentId = @segmentId AND
                SourceId = @sourceId AND
                date BETWEEN @startDate AND @endDate AND
                EXTRACT(DAYOFWEEK FROM date) IN ({daysOfWeek})
            GROUP BY Month
            ORDER BY Month ASC;";

            var parameters = new[]
            {
            new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString()),
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

        public async Task<List<DailyAverage>> GetDailyAveragesAsync(Guid segmentId, DateOnly startDate, DateOnly endDate, string daysOfWeek)
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
                SegmentId = @segmentId AND
                date BETWEEN @startDate AND @endDate AND
                EXTRACT(DAYOFWEEK FROM date) IN ({daysOfWeek})
            GROUP BY Date
            ORDER BY Date ASC;";

            var parameters = new[]
            {
            new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString()),
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

        public async Task<List<HourlySpeed>> GetHourlySpeeds(DateOnly startDate, DateOnly endDate, Guid segmentId)
        {
            string query = $@"
            SELECT *
            FROM `atspm-406601.speed_dataset.hourly_speed`
            WHERE 
                SegmentId = @segmentId AND
                date BETWEEN @startDate AND @endDate 
            ORDER BY Date ASC, BinStartTime ASC;";

            DateTime startDateTime =startDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateTime = endDate.ToDateTime(TimeOnly.MinValue);

            var parameters = new[]
            {
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId),
                new BigQueryParameter("startDate", BigQueryDbType.Date, startDateTime.Date ),
                new BigQueryParameter("endDate", BigQueryDbType.Date, endDateTime.Date),
            };

            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            return queryResults.Select(row =>
            {
                string[] formats = { "MM/dd/yyyy HH:mm:ss", "M/d/yyyy h:mm:ss tt", "yyyy-MM-ddTHH:mm:ss.fffZ" };
                var date = DateOnly.ParseExact(row["Date"].ToString(), formats);
                var time = TimeOnly.Parse(row["BinStartTime"].ToString());
                var avg = row["Average"];
                var fifteenthSpeed = row["FifteenthSpeed"];
                var eightyFifthSpeed = row["EightyFifthSpeed"];
                var ninetyFifthSpeed = row["NinetyFifthSpeed"];
                var ninetyNinthSpeed = row["NinetyNinthSpeed"];
                var violation = row["Violation"];
                var flow = row["Flow"];

                return new HourlySpeed
                {
                    Date = date.ToDateTime(new TimeOnly(0, 0)),
                    BinStartTime = date.ToDateTime(time),
                    Average = avg != null ? (long)avg : 0,
                    FifteenthSpeed = fifteenthSpeed != null ? (long)fifteenthSpeed : null,
                    EightyFifthSpeed = eightyFifthSpeed != null ? (long)eightyFifthSpeed : null,
                    NinetyFifthSpeed = ninetyFifthSpeed != null ? (long)ninetyFifthSpeed : null,
                    NinetyNinthSpeed = ninetyNinthSpeed != null ? (long)ninetyNinthSpeed : null,
                    Violation = violation != null ? (long)violation : null,
                    Flow = flow != null ? (long)flow : null
                };
            }).ToList();
        }

        public async Task<List<HourlySpeed>> GetWeeklySpeeds(DateOnly startDate, DateOnly endDate, Guid segmentId, long? sourceId)
        {
            string query = $@"
            WITH data_with_datetime AS (
              SELECT 
                *,
                TIMESTAMP(DATETIME(Date, BinStartTime)) AS datetime,
              FROM `atspm-406601.speed_dataset.hourly_speed`
              WHERE 
                SourceId = @sourceId
                AND SegmentId = @segment
                AND Date BETWEEN @startDate AND @endDate
            ),
            data_with_custom_week_start AS (
              SELECT 
                *,
              CASE
                WHEN EXTRACT(DAYOFWEEK FROM Date) = 2 THEN Date
                ELSE DATE_SUB(Date, INTERVAL EXTRACT(DAYOFWEEK FROM Date) - 2 DAY)
                END AS custom_week_start
              FROM
                  data_with_datetime
            )
            SELECT 
              custom_week_start as Date,
              COUNT(DISTINCT datetime) AS distinct_datetime_count,
              AVG(Average) AS Average,
              AVG(FifteenthSpeed) as FifteenthSpeed,
              AVG(EightyFifthSpeed) as EightyFifthSpeed,
              AVG(NinetyFifthSpeed) as NinetyFifthSpeed,
              SUM(Flow) as Flow
            FROM
                data_with_custom_week_start
            GROUP BY
                Date
            ORDER BY
                Date;";

            DateTime startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateTime = endDate.ToDateTime(TimeOnly.MinValue);

            var parameters = new[]
            {
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId),
                new BigQueryParameter("startDate", BigQueryDbType.Date, startDateTime.Date ),
                new BigQueryParameter("endDate", BigQueryDbType.Date, endDateTime.Date),
                new BigQueryParameter("sourceId", BigQueryDbType.Int64, sourceId)
            };

            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            return queryResults.Select(row =>
            {
                string[] formats = { "MM/dd/yyyy HH:mm:ss", "M/d/yyyy h:mm:ss tt", "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy/MM/dd" };
                var date = DateOnly.ParseExact(row["Date"].ToString(), formats);
                var avg = row["Average"];
                var fifteenthSpeed = row["FifteenthSpeed"];
                var eightyFifthSpeed = row["EightyFifthSpeed"];
                var ninetyFifthSpeed = row["NinetyFifthSpeed"];
                var violation = row["Violation"];
                var flow = row["Flow"];

                return new HourlySpeed
                {
                    Date = date.ToDateTime(new TimeOnly(0, 0)),
                    BinStartTime = date.ToDateTime(new TimeOnly(0, 0)),
                    Average = avg != null ? (long)avg : 0,
                    FifteenthSpeed = fifteenthSpeed != null ? (long)fifteenthSpeed : null,
                    EightyFifthSpeed = eightyFifthSpeed != null ? (long)eightyFifthSpeed : null,
                    NinetyFifthSpeed = ninetyFifthSpeed != null ? (long)ninetyFifthSpeed : null,
                    Violation = violation != null ? (long)violation : null,
                    Flow = flow != null ? (long)flow : null
                };
            }).ToList();
        }

        public async Task<List<RouteSpeed>> GetRoutesSpeeds(RouteSpeedOptions options)
        {
            var convertedStartDate = DateOnly.FromDateTime(options.StartDate);
            var convertedEndDate = DateOnly.FromDateTime(options.EndDate);
            TimeOnly convertedStartTime = new(0, 0);
            TimeOnly convertedEndTime = new(0, 0);

            if (options.StartTime.HasValue && options.EndTime.HasValue)
            {
                convertedStartTime = TimeOnly.FromDateTime(options.StartTime.Value);
                convertedEndTime = TimeOnly.FromDateTime(options.EndTime.Value);
            }

            try
            {
                var startDateTime = new DateTime(convertedStartDate.Year, convertedStartDate.Month, convertedStartDate.Day, convertedStartTime.Hour, convertedStartTime.Minute, convertedStartTime.Second);
                var end0DateTime = new DateTime(convertedEndDate.Year, convertedEndDate.Month, convertedEndDate.Day, convertedEndTime.Hour, convertedEndTime.Minute, convertedEndTime.Second);
                var query = CreateQuery(options);
                BigQueryResults queryResults;
                BigQueryParameter[] parameters;
                switch (options.AnalysisPeriod)
                {
                    case AnalysisPeriod.AllDay:
                        parameters = new[]
                        {
                            new BigQueryParameter("startDate", BigQueryDbType.Date, convertedStartDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("endDate", BigQueryDbType.Date, convertedEndDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("sourceId", BigQueryDbType.Int64, options.SourceId)
                        };
                        queryResults = await _client.ExecuteQueryAsync(query, parameters);
                        break;
                    case AnalysisPeriod.PeekPeriod:
                        parameters = new[]
                        {
                            new BigQueryParameter("startDate", BigQueryDbType.Date, convertedStartDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("endDate", BigQueryDbType.Date, convertedEndDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("sourceId", BigQueryDbType.Int64, options.SourceId)
                        };

                        queryResults = await _client.ExecuteQueryAsync(query, parameters);
                        break;
                    case AnalysisPeriod.CustomHour:
                        parameters = new[]
                        {
                            new BigQueryParameter("startDate", BigQueryDbType.Date, convertedStartDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("endDate", BigQueryDbType.Date, convertedEndDate.ToDateTime(new TimeOnly(0, 0))),
                            new BigQueryParameter("startTime", BigQueryDbType.Time, convertedStartTime.ToTimeSpan()),
                            new BigQueryParameter("endTime", BigQueryDbType.Time, convertedEndTime.ToTimeSpan()),
                            new BigQueryParameter("sourceId", BigQueryDbType.Int64, options.SourceId)
                        };
                        queryResults = await _client.ExecuteQueryAsync(query, parameters);
                        break;
                    default: throw new NotSupportedException();
                }

                List<RouteSpeed> results = new List<RouteSpeed>();
                foreach (BigQueryRow row in queryResults)
                {
                    results.Add(TransformRowToRouteSpeed(row));
                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        private static RouteSpeed TransformRowToRouteSpeed(BigQueryRow row)
        {
            var reader = new WKTReader();
            // Access row data as needed
            var segmentId = row["SegmentId"];
            var sourceId = row["SourceId"];
            var avg = row["Avg"];
            var percentile15 = row["Percentilespd_15"];
            var percentile85 = row["Percentilespd_85"];
            var percentile95 = row["Percentilespd_95"];
            var flow = row["Flow"];
            var estimatedViolations = row["EstimatedViolations"];
            var speedLimit = row["SpeedLimit"];
            var name = row["Name"];
            var wkt = (string)row["Shape"];
            // Further processing...
            Geometry shape = wkt != null ? reader.Read(wkt) : null;

            return new RouteSpeed
            {
                SegmentId = segmentId != null ? segmentId.ToString() : "",
                SourceId = sourceId != null ? (long)sourceId : 0,
                Name = name != null ? name.ToString() : "",
                Avg = avg != null ? Math.Round((double)avg, 2) : null,
                Percentilespd_15 = percentile15 != null ? (long)percentile15 : null,
                Percentilespd_85 = percentile85 != null ? (long)percentile85 : null,
                Percentilespd_95 = percentile95 != null ? (long)percentile95 : null,
                Flow = flow != null ? (long)flow : null,
                EstimatedViolations = estimatedViolations != null ? (long)estimatedViolations : null,
                SpeedLimit = speedLimit != null ? (long)speedLimit : 0,
                Shape = shape,
            };
        }

        private static string CreateQuery(RouteSpeedOptions options)
        {
            var daysOfWeekCondition = string.Join(", ", options.DaysOfWeek.Select(day => ((int)day + 1).ToString()));

            var baseQuery = $@"
                            WITH FilteredSegments AS (
                                SELECT
                                    s.Id,
                                    se.SourceId,
                                    s.SpeedLimit,
                                    s.Name,
                                    ST_AsText(s.Shape) AS Shape
                                FROM
                                    `atspm-406601.speed_dataset.segment` AS s
                                JOIN
                                    `atspm-406601.speed_dataset.segment_entity` AS se
                                ON
                                    s.Id = se.SegmentId
                                WHERE
                                    se.SourceId = @sourceId
                            ),
                            RouteStats AS (
                                SELECT
                                    fs.Id AS SegmentId,
                                    fs.SourceId,
                                    AVG(hs.Average) AS Avg,
                                    APPROX_QUANTILES(hs.FifteenthSpeed, 100)[ORDINAL(85)] AS Percentilespd_15,
                                    APPROX_QUANTILES(hs.EightyFifthSpeed, 100)[ORDINAL(85)] AS Percentilespd_85,
                                    APPROX_QUANTILES(hs.NinetyFifthSpeed, 100)[ORDINAL(85)] AS Percentilespd_95,
                                    SUM(hs.Flow) AS Flow,
                                    fs.SpeedLimit,
                                    fs.Name,
                                    fs.Shape
                                FROM
                                    FilteredSegments AS fs
                                LEFT JOIN (
                                    SELECT *
                                    FROM `atspm-406601.speed_dataset.hourly_speed`";

            var groupByClause = @"
                                GROUP BY
                                    fs.Id, fs.SourceId, fs.SpeedLimit, fs.Name, fs.Shape
                                )";

            var selectClause = $@"SELECT
                                    rs.SegmentId,
                                    rs.SourceId,
                                    rs.Avg,
                                    rs.Percentilespd_15,
                                    rs.Percentilespd_85,
                                    rs.Percentilespd_95,
                                    rs.Flow,
                                    rs.SpeedLimit,
                                    rs.Name,
                                    rs.Shape,
                                    IFNULL(
                                        SAFE_CAST(
                                            ROUND(SUM(
                                                CASE 
                                                    WHEN rs.Percentilespd_15 >= rs.SpeedLimit THEN 0.85 * rs.Flow
                                                    WHEN rs.Avg >= rs.SpeedLimit THEN 0.5 * rs.Flow
                                                    WHEN rs.Percentilespd_85 >= rs.SpeedLimit THEN 0.15 * rs.Flow
                                                    WHEN rs.Percentilespd_95 >= rs.SpeedLimit THEN 0.05 * rs.Flow
                                                    ELSE 0
                                                END
                                            ) / NULLIF(SUM(rs.Flow), 0)) AS INT64), 0) AS EstimatedViolations
                                FROM
                                    RouteStats AS rs
                                GROUP BY
                                    rs.SegmentId, rs.SourceId, rs.Avg, rs.Percentilespd_15, rs.Percentilespd_85, rs.Percentilespd_95, rs.Flow, rs.SpeedLimit, rs.Name, rs.Shape;";

            string onClause = "ON fs.Id = hs.SegmentId";

            string whereClause = "";

            switch (options.AnalysisPeriod)
            {
                case AnalysisPeriod.AllDay:
                    whereClause = $@"
                                    WHERE DATE BETWEEN @startDate AND @endDate
                                    AND EXTRACT(DAYOFWEEK FROM DATE) IN ({daysOfWeekCondition})
                                    AND SourceId = @sourceId
                                ) AS hs
                                ";
                    break;
                case AnalysisPeriod.PeekPeriod:
                    whereClause = $@"
                                    WHERE DATE BETWEEN @startDate AND @endDate AND
                                    (
                                        (TIME(BinStartTime) BETWEEN TIME '06:00:00' AND TIME '09:00:00') OR
                                        (TIME(BinStartTime) BETWEEN TIME '15:00:00' AND TIME '18:00:00')
                                    ) AND
                                    EXTRACT(DAYOFWEEK FROM DATE) IN ({daysOfWeekCondition})
                                    AND SourceId = @sourceId
                                ) AS hs
                                ";
                    break;
                case AnalysisPeriod.CustomHour:
                    whereClause = $@"
                                    WHERE DATE BETWEEN @startDate AND @endDate
                                    AND TIME(BinStartTime) BETWEEN @startTime AND @endTime
                                    AND EXTRACT(DAYOFWEEK FROM DATE) IN ({daysOfWeekCondition})
                                    AND SourceId = @sourceId
                                ) AS hs
                                ";
                    break;
                default:
                    break;
            }
            var finalQuery = baseQuery + whereClause + onClause + groupByClause + selectClause;
            return finalQuery;
            //            WITH FilteredSegments AS(
            //    SELECT
            //        s.Id,
            //            se.SourceId,
            //            s.SpeedLimit,
            //        s.Name,
            //        ST_AsText(s.Shape) AS Shape
            //    FROM
            //        `atspm - 406601.speed_dataset.segment` AS s
            //    JOIN
            //        `atspm - 406601.speed_dataset.segment_entity` AS se
            //    ON
            //        s.Id = se.SegmentId
            //    WHERE
            //        se.SourceId = @sourceId
            //),
            //RouteStats AS(
            //    SELECT
            //        fs.Id AS SegmentId,
            //        fs.SourceId,
            //        AVG(hs.Average) AS Avg,
            //        APPROX_QUANTILES(hs.FifteenthSpeed, 100)[ORDINAL(85)] AS Percentilespd_15,
            //        APPROX_QUANTILES(hs.EightyFifthSpeed, 100)[ORDINAL(85)] AS Percentilespd_85,
            //        APPROX_QUANTILES(hs.NinetyFifthSpeed, 100)[ORDINAL(85)] AS Percentilespd_95,
            //            SUM(hs.Flow) AS Flow,
            //        fs.SpeedLimit,
            //        fs.Name,
            //        fs.Shape
            //    FROM
            //        FilteredSegments AS fs
            //    LEFT JOIN(
            //            SELECT *
            //        FROM `atspm - 406601.speed_dataset.hourly_speed`
            //        WHERE DATE BETWEEN @startDate AND @endDate
            //          AND TIME(BinStartTime) BETWEEN @startTime AND @endTime
            //          AND EXTRACT(DAYOFWEEK FROM DATE) IN(1, 2, 3, 4, 5, 6, 7)
            //    ) AS hs
            //    ON fs.Id = hs.SegmentId
            //    GROUP BY
            //        fs.Id, fs.SourceId, fs.SpeedLimit, fs.Name, fs.Shape
            //)
            //SELECT
            //    rs.SegmentId,
            //    rs.SourceId,
            //    rs.Avg,
            //    rs.Percentilespd_15,
            //    rs.Percentilespd_85,
            //    rs.Percentilespd_95,
            //    rs.Flow,
            //    rs.SpeedLimit,
            //    rs.Name,
            //    rs.Shape,
            //    IFNULL(
            //        SAFE_CAST(
            //            ROUND(SUM(
            //                CASE
            //                    WHEN rs.Percentilespd_15 >= rs.SpeedLimit THEN 0.85 * rs.Flow
            //                    WHEN rs.Avg >= rs.SpeedLimit THEN 0.5 * rs.Flow
            //                    WHEN rs.Percentilespd_85 >= rs.SpeedLimit THEN 0.15 * rs.Flow
            //                    WHEN rs.Percentilespd_95 >= rs.SpeedLimit THEN 0.05 * rs.Flow
            //                    ELSE 0
            //                END
            //            ) / NULLIF(SUM(rs.Flow), 0)) AS INT64), 0) AS EstimatedViolations
            //FROM
            //    RouteStats AS rs
            //GROUP BY
            //    rs.SegmentId, rs.SourceId, rs.Avg, rs.Percentilespd_15, rs.Percentilespd_85, rs.Percentilespd_95, rs.Flow, rs.SpeedLimit, rs.Name, rs.Shape;

            //query = $@"
            //                    WITH RouteStats AS (
            //                        SELECT
            //                            hs.SegmentId,
            //                            AVG(hs.Average) AS Avg,
            //                            APPROX_QUANTILES(hs.FifteenthSpeed, 100)[ORDINAL(85)] AS Percentilespd_15,
            //                            APPROX_QUANTILES(hs.EightyFifthSpeed, 100)[ORDINAL(85)] AS Percentilespd_85,
            //                            APPROX_QUANTILES(hs.NinetyFifthSpeed, 100)[ORDINAL(85)] AS Percentilespd_95,
            //                            SUM(hs.Flow) AS Flow
            //                        FROM
            //                            `atspm-406601.speed_dataset.hourly_speed` AS hs
            //                        WHERE
            //                            DATE BETWEEN @startDate AND @endDate
            //                            AND TIME(hs.BinStartTime) BETWEEN @startTime AND @endTime
            //                            AND EXTRACT(DAYOFWEEK FROM DATE) IN (1,2,3,4,5,6,7)
            //                        GROUP BY
            //                            hs.SegmentId
            //                    )
            //                    SELECT
            //                        rs.SegmentId,
            //                        rs.Avg,
            //                        rs.Percentilespd_15,
            //                        rs.Percentilespd_85,
            //                        rs.Percentilespd_95,
            //                        rs.Flow,
            //                        r.SpeedLimit,
            //                        r.Name,
            //                        ANY_VALUE(ST_AsText(r.Shape)) AS Shape,
            //                        IFNULL(
            //                            SAFE_CAST(
            //                                ROUND(SUM
            //                                (CASE 
            //                                  WHEN rs.Percentilespd_15 >= r.SpeedLimit THEN 0.85 * rs.Flow
            //                                  WHEN rs.Avg >= r.SpeedLimit THEN 0.5 * rs.Flow
            //                                  WHEN rs.Percentilespd_85 >= r.SpeedLimit THEN 0.15 * rs.Flow
            //                                  WHEN rs.Percentilespd_95 >= r.SpeedLimit THEN 0.05 * rs.Flow
            //                                  ELSE 0
            //                                  END) / NULLIF(SUM(rs.Flow), 0)) AS INT64), 0) AS EstimatedViolations
            //                    FROM
            //                        RouteStats AS rs
            //                    JOIN
            //                        `atspm-406601.speed_dataset.segment` AS r
            //                    ON
            //                        rs.SegmentId = r.Id
            //                    GROUP BY
            //                        rs.SegmentId, rs.Avg, rs.Percentilespd_15, rs.Percentilespd_85, rs.Percentilespd_95, rs.Flow, r.SpeedLimit, r.Name;";
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
            var oldRow = await LookupAsync(new { item.Date, item.BinStartTime, item.SegmentId });
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

                queryBuilder.Append($" WHERE Date = @Date AND BinStartTime = @BinStartTime AND SegmentId = @SegmentId");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@Date", BigQueryDbType.DateTime, item.Date),
            new BigQueryParameter("@BinStartTime", BigQueryDbType.DateTime, item.BinStartTime),
            new BigQueryParameter("@SegmentId", BigQueryDbType.Int64, item.SegmentId)
        };

                _client.ExecuteQuery(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Date, BinStartTime, SegmentId, SourceId, ConfidenceId, Average, FifteenthSpeed, EightyFifthSpeed, NinetyFifthSpeed, NinetyNinthSpeed, Violation, Flow) " +
                    $"VALUES (" +
                    $"'{item.Date:O}', " +
                    $"'{item.BinStartTime:O}', " +
                    $"{item.SegmentId}, " +
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
            var oldRow = await LookupAsync(new { item.Date, item.BinStartTime, item.SegmentId });
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

                queryBuilder.Append($" WHERE Date = @Date AND BinStartTime = @BinStartTime AND SegmentId = @SegmentId");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@Date", BigQueryDbType.DateTime, item.Date),
            new BigQueryParameter("@BinStartTime", BigQueryDbType.DateTime, item.BinStartTime),
            new BigQueryParameter("@SegmentId", BigQueryDbType.Int64, item.SegmentId)
        };

                _client.ExecuteQueryAsync(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Date, BinStartTime, SegmentId, SourceId, ConfidenceId, Average, FifteenthSpeed, EightyFifthSpeed, NinetyFifthSpeed, NinetyNinthSpeed, Violation, Flow) " +
                    $"VALUES (" +
                    $"'{item.Date:O}', " +
                    $"'{item.BinStartTime:O}', " +
                    $"{item.SegmentId}, " +
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