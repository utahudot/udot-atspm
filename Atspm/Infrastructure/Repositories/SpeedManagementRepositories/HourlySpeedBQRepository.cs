using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories
{
    ///<inheritdoc cref="IHourlySpeedRepository"/>
    public class HourlySpeedBQRepository : ATSPMRepositoryBQBase<HourlySpeed>, IHourlySpeedRepository
    {

        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private BigQueryTable _table;
        private readonly ILogger<ATSPMRepositoryBQBase<HourlySpeed>> _logger;

        public HourlySpeedBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<HourlySpeed>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
            _table = _client.GetTable(_datasetId, _tableId);
        }


        public async Task AddHourlySpeedAsync(HourlySpeed hourlySpeed)
        {
            var insertRow = CreateRow(hourlySpeed);
            await _table.InsertRowAsync(insertRow);
        }

        public async Task AddHourlySpeedsAsync(List<HourlySpeed> hourlySpeeds)
        {
            try
            {
                List<BigQueryInsertRow> insertRows = new List<BigQueryInsertRow>();
                int batchSize = 1000;

                foreach (var hourlySpeed in hourlySpeeds)
                {
                    try
                    {
                        insertRows.Add(CreateRow(hourlySpeed));

                        if (insertRows.Count >= batchSize)
                        {
                            await _table.InsertRowsAsync(insertRows);
                            insertRows.Clear(); // Clear the list for the next batch
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("THERE WAS AN ERROR CREATING THE ROW - " + ex.ToString());
                    }
                }

                // Insert any remaining rows that didn't make a full batch
                if (insertRows.Count > 0)
                {
                    await _table.InsertRowsAsync(insertRows);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("This means that there was an error in the AddHourlySpeedsAsync - " + ex.ToString());
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
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString()),
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

        public async Task<List<HourlySpeed>> GetHourlySpeedsWithFiltering(List<Guid> segmentIds, DateTime startDate, DateTime endDate, DateTime? startTime = null, DateTime? endTime = null, int? dayOfWeek = null, List<DateTime>? specificDays = null)
        {
            var ids = string.Join(", ", segmentIds.Select(id => $"'{id}'"));
            string query = $@"SELECT * FROM `{_datasetId}.{_tableId}` WHERE 
                    SegmentId IN ({ids}) AND
                    Date BETWEEN @startDate AND @endDate";
            if (startTime != null && endTime != null)
            {
                DateTime startTimeOut = (DateTime)startTime;
                DateTime endTimeOut = (DateTime)endTime;
                query = query + $" AND TIME(BinStartTime) BETWEEN '{startTimeOut.TimeOfDay.ToString(@"hh\:mm\:ss")}' AND '{endTimeOut.TimeOfDay.ToString(@"hh\:mm\:ss")}'";
            }
            if (specificDays != null && specificDays.Count > 0)
            {
                var dates = string.Join(", ", specificDays.Select(d => $"'{d.ToString("yyyy-MM-dd")}'"));
                query = query + $" AND Date IN ({dates})";
            }
            if (dayOfWeek != null && dayOfWeek < 8 && dayOfWeek > 0)
            {
                query = query + $" AND EXTRACT(DAYOFWEEK FROM `Date`) = {dayOfWeek}";
            }

            query = query + " ORDER BY Date ASC, BinStartTime ASC;";

            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("startDate", BigQueryDbType.Date, startDate.Date),
                new BigQueryParameter("endDate", BigQueryDbType.Date, endDate.Date)
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
                {"Date", item.Date.AsBigQueryDate() },
                {"BinStartTime", item.BinStartTime.TimeOfDay },
                {"SegmentId", item.SegmentId.ToString() },
                {"SourceId", item.SourceId },
                {"PercentObserved", item.PercentObserved },
                {"Average", item.Average },
                {"FifteenthSpeed", item.FifteenthSpeed },
                {"EightyFifthSpeed", item.EightyFifthSpeed },
                {"NinetyFifthSpeed", item.NinetyFifthSpeed },
                {"NinetyNinthSpeed", item.NinetyNinthSpeed },
                {"Violation", item.Violation },
                {"ExtremeViolation", item.ExtremeViolation },
                {"Flow", item.Flow },
                {"MinSpeed", item.MinSpeed },
                {"MaxSpeed", item.MaxSpeed },
                {"SourceDataAnalyzed", item.SourceDataAnalyzed },
            };
        }

        protected override HourlySpeed MapRowToEntity(BigQueryRow row)
        {
            string[] formats = { "MM/dd/yyyy HH:mm:ss", "M/d/yyyy h:mm:ss tt", "yyyy-MM-ddTHH:mm:ss.fffZ" };
            var bigQueryDate = DateOnly.ParseExact(row["Date"].ToString(), formats);
            var bigQueryBinStartTime = TimeOnly.Parse(row["BinStartTime"].ToString());
            var bigQuerySegmentId = Guid.Parse(row["SegmentId"].ToString());
            var bigQuerySourceId = long.Parse(row["SourceId"].ToString());
            var bigQueryPercentObservedId = row["PercentObserved"] != null ? double.Parse(row["PercentObserved"].ToString()) : (double?)null;
            var bigQueryAverage = double.Parse(row["Average"].ToString());
            var bigQueryFifteenthSpeed = row["FifteenthSpeed"] != null ? double.Parse(row["FifteenthSpeed"].ToString()) : (double?)null;
            var bigQueryEightyFifthSpeed = row["EightyFifthSpeed"] != null ? double.Parse(row["EightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryNinetyFifthSpeed = row["NinetyFifthSpeed"] != null ? double.Parse(row["NinetyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryNinetyNinthSpeed = row["NinetyNinthSpeed"] != null ? double.Parse(row["NinetyNinthSpeed"].ToString()) : (double?)null;
            var bigQueryViolation = row["Violation"] != null ? long.Parse(row["Violation"].ToString()) : (long?)null;
            var bigQueryExtremeViolation = row["ExtremeViolation"] != null ? long.Parse(row["ExtremeViolation"].ToString()) : (long?)null;
            var bigQueryFlow = row["Flow"] != null ? long.Parse(row["Flow"].ToString()) : (long?)null;
            var bigQueryMinSpeed = row["MinSpeed"] != null ? double.Parse(row["MinSpeed"].ToString()) : (double?)null;
            var bigQueryMaxSpeed = row["MaxSpeed"] != null ? double.Parse(row["MaxSpeed"].ToString()) : (double?)null;
            var sourceDataAnalyzed = row["SourceDataAnalyzed"] != null ? bool.Parse(row["SourceDataAnalyzed"].ToString()) : (bool?)null;

            return new HourlySpeed
            {
                Date = bigQueryDate.ToDateTime(new TimeOnly(0, 0)),
                BinStartTime = bigQueryDate.ToDateTime(bigQueryBinStartTime),
                SegmentId = bigQuerySegmentId,
                SourceId = bigQuerySourceId,
                PercentObserved = bigQueryPercentObservedId,
                Average = bigQueryAverage,
                FifteenthSpeed = bigQueryFifteenthSpeed,
                EightyFifthSpeed = bigQueryEightyFifthSpeed,
                NinetyFifthSpeed = bigQueryNinetyFifthSpeed,
                NinetyNinthSpeed = bigQueryNinetyNinthSpeed,
                Violation = bigQueryViolation,
                ExtremeViolation = bigQueryExtremeViolation,
                Flow = bigQueryFlow,
                MinSpeed = bigQueryMinSpeed,
                MaxSpeed = bigQueryMaxSpeed,
                SourceDataAnalyzed = sourceDataAnalyzed
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
                AVG(ExtremeViolation) AS ExtremeViolation,
                MIN(MinSpeed) AS MinSpeed,
                MAX(MaxSpeed) AS MaxSpeed
            FROM `{_datasetId}.{_tableId}`
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
                ExtremeViolation = Convert.ToDouble(row["ExtremeViolation"]),
                Flow = Convert.ToDouble(row["Flow"]),
                MaxSpeed = Convert.ToDouble(row["MaxSpeed"]),
                MinSpeed = Convert.ToDouble(row["MinSpeed"]),
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
                AVG(ExtremeViolation) AS ExtremeViolation,
                AVG(Flow) AS Flow,
                MIN(MinSpeed) AS MinSpeed,
                MAX(MaxSpeed) AS MaxSpeed
            FROM `{_datasetId}.{_tableId}`
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
                ExtremeViolation = Convert.ToDouble(row["ExtremeViolation"]),
                Flow = Convert.ToDouble(row["Flow"]),
                MaxSpeed = Convert.ToDouble(row["MaxSpeed"]),
                MinSpeed = Convert.ToDouble(row["MinSpeed"])
            }).ToList();
        }

        public async Task<List<HourlySpeed>> GetHourlySpeedsForSegmentInSource(OptionsBase baseOptions, Guid segmentId)
        {
            return await GetSpeedsInternal(baseOptions, segmentId, isWeekly: false);
        }

        public async Task<List<HourlySpeed>> GetWeeklySpeedsForSegmentInSource(OptionsBase baseOptions, Guid segmentId)
        {
            return await GetSpeedsInternal(baseOptions, segmentId, isWeekly: true);
        }

        private async Task<List<HourlySpeed>> GetSpeedsInternal(OptionsBase baseOptions, Guid segmentId, bool isWeekly)
        {
            string query = isWeekly ? GetWeeklyQuery() : GetHourlyQuery();

            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString()),
                new BigQueryParameter("startDate", BigQueryDbType.Date, baseOptions.StartDate.ToDateTime(TimeOnly.MinValue).Date),
                new BigQueryParameter("endDate", BigQueryDbType.Date, baseOptions.EndDate.ToDateTime(TimeOnly.MinValue).Date),
                new BigQueryParameter("sourceId", BigQueryDbType.Int64, baseOptions.SourceId)
            };

            var queryResults = await _client.ExecuteQueryAsync(query, parameters.ToArray());
            return queryResults.Select(row => ParseHourlySpeed(row, isWeekly)).ToList();
        }

        private static HourlySpeed ParseHourlySpeed(BigQueryRow row, bool isWeekly)
        {
            string[] formats = { "MM/dd/yyyy HH:mm:ss", "M/d/yyyy h:mm:ss tt", "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy/MM/dd" };
            var date = DateOnly.ParseExact(row["Date"].ToString(), formats);

            return new HourlySpeed
            {
                Date = date.ToDateTime(new TimeOnly(0, 0)),
                BinStartTime = isWeekly ? date.ToDateTime(new TimeOnly(0, 0)) : date.ToDateTime(TimeOnly.Parse(row["BinStartTime"].ToString())),
                Average = row["Average"] != null ? Convert.ToDouble(row["Average"]) : 0,
                FifteenthSpeed = row["FifteenthSpeed"] != null ? Convert.ToDouble(row["FifteenthSpeed"]) : null,
                EightyFifthSpeed = row["EightyFifthSpeed"] != null ? Convert.ToDouble(row["EightyFifthSpeed"]) : null,
                NinetyFifthSpeed = row["NinetyFifthSpeed"] != null ? Convert.ToDouble(row["NinetyFifthSpeed"]) : null,
                MinSpeed = row["MinSpeed"] != null ? Convert.ToDouble(row["MinSpeed"]) : null,
                MaxSpeed = row["MaxSpeed"] != null ? Convert.ToDouble(row["MaxSpeed"]) : null,
                Violation = row["Violation"] != null ? (long)row["Violation"] : null,
                ExtremeViolation = row["ExtremeViolation"] != null ? (long)row["ExtremeViolation"] : null,
                Flow = row["Flow"] != null ? (long)row["Flow"] : null,
                SourceDataAnalyzed = row["SourceDataAnalyzed"] != null ? (bool)row["SourceDataAnalyzed"] : null,
            };
        }

        private string GetHourlyQuery()
        {
            return $@"
            SELECT *
            FROM `{_datasetId}.{_tableId}`
            WHERE 
                SegmentId = @segmentId AND
                date BETWEEN @startDate AND @endDate 
                AND SourceId = @sourceId
            ORDER BY Date ASC, BinStartTime ASC;";
        }

        private string GetWeeklyQuery()
        {
            return $@"
            WITH data_with_datetime AS (
              SELECT 
                *,
                TIMESTAMP(DATETIME(Date, BinStartTime)) AS datetime
              FROM `{_datasetId}.{_tableId}`
              WHERE 
                SourceId = @sourceId
                AND SegmentId = @segmentId
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
              SUM(Flow) as Flow,
              SUM(ExtremeViolation) as ExtremeViolation,
              SUM(Violation) as Violation
            FROM
              data_with_custom_week_start
            GROUP BY
              Date
            ORDER BY
              Date;";
        }

        public async Task<List<RouteSpeed>> GetRoutesSpeeds(RouteSpeedOptions options)
        {
            var convertedStartDate = DateOnly.FromDateTime(options.StartDate);
            var convertedEndDate = DateOnly.FromDateTime(options.EndDate);
            TimeOnly convertedStartTime = TimeOnly.FromDateTime(options.StartTime);
            TimeOnly convertedEndTime = TimeOnly.FromDateTime(options.EndTime);
            List<BigQueryParameter> parameters = new List<BigQueryParameter>();

            GetOptionalParams(options, parameters);

            try
            {
                var query = CreateQuery(options);
                BigQueryResults queryResults;

                parameters.AddRange(new BigQueryParameter[]
                {
                    new BigQueryParameter("startDate", BigQueryDbType.Date, convertedStartDate.ToDateTime(new TimeOnly(0, 0))),
                    new BigQueryParameter("endDate", BigQueryDbType.Date, convertedEndDate.ToDateTime(new TimeOnly(0, 0))),
                    new BigQueryParameter("startTime", BigQueryDbType.Time, convertedStartTime.ToTimeSpan()),
                    new BigQueryParameter("endTime", BigQueryDbType.Time, convertedEndTime.ToTimeSpan()),
                    new BigQueryParameter("sourceId", BigQueryDbType.Int64, options.SourceId)
                });
                queryResults = await _client.ExecuteQueryAsync(query, parameters);

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

        private void GetOptionalParams(RouteSpeedOptions options, List<BigQueryParameter> parameters)
        {
            if (!IsNullOrEmpty(options.City))
            {
                parameters.Add(new BigQueryParameter("city", BigQueryDbType.String, options.City));
            }
            if (!IsNullOrEmpty(options.County))
            {
                parameters.Add(new BigQueryParameter("county", BigQueryDbType.String, options.County));
            }
            if (!IsNullOrEmpty(options.Region))
            {
                parameters.Add(new BigQueryParameter("region", BigQueryDbType.String, options.Region));
            }
            if (!IsNullOrEmpty(options.AccessCategory))
            {
                parameters.Add(new BigQueryParameter("accessCategory", BigQueryDbType.String, options.AccessCategory));
            }
            if (!IsNullOrEmpty(options.FunctionalType))
            {
                parameters.Add(new BigQueryParameter("functionalType", BigQueryDbType.String, options.FunctionalType));
            }
        }

        private bool IsNullOrEmpty(string val)
        {
            return val == null || val.Length == 0;
        }

        private static RouteSpeed TransformRowToRouteSpeed(BigQueryRow row)
        {
            var reader = new WKTReader();
            // Access row data as needed
            var segmentId = Guid.Parse(row["SegmentId"].ToString());
            var sourceId = row["SourceId"];
            var avg = row["Avg"] != null ? double.Parse(row["Avg"].ToString()) : (double?)null;
            var percentile15 = row["Percentilespd_15"] != null ? double.Parse(row["Percentilespd_15"].ToString()) : (double?)null;
            var percentile85 = row["Percentilespd_85"] != null ? double.Parse(row["Percentilespd_85"].ToString()) : (double?)null;
            var percentile95 = row["Percentilespd_95"] != null ? double.Parse(row["Percentilespd_95"].ToString()) : (double?)null;
            var sourceDataAnalyzed = row["SourceDataAnalyzed"] != null ? bool.Parse(row["SourceDataAnalyzed"].ToString()) : (bool?)null;
            var flow = row["Flow"];
            var minSpeed = row["MinSpeed"];
            var maxSpeed = row["MaxSpeed"];
            var estimatedViolations = row["EstimatedViolations"];
            var speedLimit = row["SpeedLimit"];
            var name = row["Name"];
            var wkt = (string)row["Shape"];

            Geometry shape = wkt != null ? reader.Read(wkt) : null;

            return new RouteSpeed
            {
                SegmentId = segmentId != null ? segmentId.ToString() : "",
                SourceId = sourceId != null ? (long)sourceId : 0,
                Name = name != null ? name.ToString() : "",
                Avg = avg != null ? Math.Round((double)avg, 2) : null,
                Percentilespd_15 = percentile15 != null ? (double)percentile15 : null,
                Percentilespd_85 = percentile85 != null ? (double)percentile85 : null,
                Percentilespd_95 = percentile95 != null ? (double)percentile95 : null,
                Flow = flow != null ? (long)flow : null,
                EstimatedViolations = estimatedViolations != null ? (long)estimatedViolations : null,
                SpeedLimit = speedLimit != null ? (long)speedLimit : 0,
                Shape = shape,
                MinSpeed = minSpeed != null ? (double)minSpeed : null,
                MaxSpeed = maxSpeed != null ? (double)maxSpeed : null
            };
        }

        private string CreateQuery(RouteSpeedOptions options)
        {
            var daysOfWeekCondition = string.Join(", ", options.DaysOfWeek.Select(day => ((int)day + 1).ToString()));
            var filteredSegmentsQuery = GetFilteredSegmentsQuery(options);

            var baseQuery = $@"
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
                                    FROM `{_datasetId}.{_tableId}`";

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

            string whereClause = $@"
                                    WHERE DATE BETWEEN @startDate AND @endDate
                                    AND TIME(BinStartTime) BETWEEN @startTime AND @endTime
                                    AND EXTRACT(DAYOFWEEK FROM DATE) IN ({daysOfWeekCondition})
                                    AND SourceId = @sourceId
                                ) AS hs ";

            var finalQuery = filteredSegmentsQuery + baseQuery + whereClause + onClause + groupByClause + selectClause;
            return finalQuery;
        }

        private string GetFilteredSegmentsQuery(RouteSpeedOptions options)
        {
            var whereClause = new StringBuilder();
            bool hasPreviousCondition = true;

            whereClause.AppendLine(" WHERE ");
            whereClause.AppendLine(" se.SourceId = @sourceId ");

            if (!string.IsNullOrEmpty(options.City))
            {
                if (hasPreviousCondition) whereClause.Append(" AND ");
                whereClause.AppendLine($"s.City = @city");
                hasPreviousCondition = true;
            }

            if (!string.IsNullOrEmpty(options.County))
            {
                if (hasPreviousCondition) whereClause.Append(" AND ");
                whereClause.AppendLine($"s.County = @county");
                hasPreviousCondition = true;
            }

            if (!string.IsNullOrEmpty(options.Region))
            {
                if (hasPreviousCondition) whereClause.Append(" AND ");
                whereClause.AppendLine($"s.Region = @region");
                hasPreviousCondition = true;
            }

            if (!string.IsNullOrEmpty(options.AccessCategory))
            {
                if (hasPreviousCondition) whereClause.Append(" AND ");
                whereClause.AppendLine($"s.AccessCategory = @accessCategory");
                hasPreviousCondition = true;
            }

            if (!string.IsNullOrEmpty(options.FunctionalType))
            {
                if (hasPreviousCondition) whereClause.Append(" AND ");
                whereClause.AppendLine($"s.FunctionalType = @functionalType");
            }
            whereClause.AppendLine(" ),");

            var filteredSegments = new StringBuilder();
            filteredSegments.Append($@"
            WITH FilteredSegments AS (
                SELECT
                    s.Id,
                    se.SourceId,
                    s.SpeedLimit,
                    s.Name,
                    ST_AsText(s.Shape) AS Shape
                FROM
                    `atspm-406601.speedDataset.segment` AS s
                JOIN
                    `atspm-406601.speedDataset.segmentEntity` AS se
                ON
                    s.Id = se.SegmentId");

            return filteredSegments.Append(whereClause).ToString();
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
                queryBuilder.Append($"PercentObserved = {item.PercentObserved}, ");
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

                if (item.ExtremeViolation.HasValue)
                {
                    queryBuilder.Append($"ExtremeViolation = {item.ExtremeViolation}, ");
                }
                else
                {
                    queryBuilder.Append($"ExtremeViolation = NULL, ");
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
            new BigQueryParameter("@SegmentId", BigQueryDbType.String, item.SegmentId.ToString())
        };

                _client.ExecuteQuery(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Date, BinStartTime, SegmentId, SourceId, PercentObserved, Average, FifteenthSpeed, EightyFifthSpeed, NinetyFifthSpeed, NinetyNinthSpeed, Violation, ExtremeViolation, Flow) " +
                    $"VALUES (" +
                    $"'{item.Date:O}', " +
                    $"'{item.BinStartTime:O}', " +
                    $"{item.SegmentId}, " +
                    $"{item.SourceId}, " +
                    $"{item.PercentObserved}, " +
                    $"{item.Average}, " +
                    $"{(item.FifteenthSpeed.HasValue ? item.FifteenthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.EightyFifthSpeed.HasValue ? item.EightyFifthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.NinetyFifthSpeed.HasValue ? item.NinetyFifthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.NinetyNinthSpeed.HasValue ? item.NinetyNinthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.Violation.HasValue ? item.Violation.Value.ToString() : "NULL")}, " +
                    $"{(item.ExtremeViolation.HasValue ? item.ExtremeViolation.Value.ToString() : "NULL")}, " +
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
                queryBuilder.Append($"PercentObserved = {item.PercentObserved}, ");
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

                if (item.ExtremeViolation.HasValue)
                {
                    queryBuilder.Append($"ExtremeViolation = {item.ExtremeViolation}, ");
                }
                else
                {
                    queryBuilder.Append($"ExtremeViolation = NULL, ");
                }

                if (item.Flow.HasValue)
                {
                    queryBuilder.Append($"Flow = {item.Flow}, ");
                }
                else
                {
                    queryBuilder.Append($"Flow = NULL, ");
                }

                if (item.Flow.HasValue)
                {
                    queryBuilder.Append($"MinSpeed = {item.MinSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"MinSpeed = NULL, ");
                }

                if (item.Flow.HasValue)
                {
                    queryBuilder.Append($"MaxSpeed = {item.MaxSpeed}, ");
                }
                else
                {
                    queryBuilder.Append($"MaxSpeed = NULL, ");
                }

                // Remove the last comma and space
                queryBuilder.Length -= 2;

                queryBuilder.Append($" WHERE Date = @Date AND BinStartTime = @BinStartTime AND SegmentId = @SegmentId");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@Date", BigQueryDbType.DateTime, item.Date),
            new BigQueryParameter("@BinStartTime", BigQueryDbType.DateTime, item.BinStartTime),
            new BigQueryParameter("@SegmentId", BigQueryDbType.String, item.SegmentId)
        };

                _client.ExecuteQueryAsync(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Date, BinStartTime, SegmentId, SourceId, PercentObserved, Average, FifteenthSpeed, EightyFifthSpeed, NinetyFifthSpeed, NinetyNinthSpeed, Violation, Flow) " +
                    $"VALUES (" +
                    $"'{item.Date:O}', " +
                    $"'{item.BinStartTime:O}', " +
                    $"{item.SegmentId}, " +
                    $"{item.SourceId}, " +
                    $"{item.PercentObserved}, " +
                    $"{item.Average}, " +
                    $"{(item.FifteenthSpeed.HasValue ? item.FifteenthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.EightyFifthSpeed.HasValue ? item.EightyFifthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.NinetyFifthSpeed.HasValue ? item.NinetyFifthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.NinetyNinthSpeed.HasValue ? item.NinetyNinthSpeed.Value.ToString() : "NULL")}, " +
                    $"{(item.Violation.HasValue ? item.Violation.Value.ToString() : "NULL")}, " +
                    $"{(item.ExtremeViolation.HasValue ? item.ExtremeViolation.Value.ToString() : "NULL")}, " +
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

        public async Task<List<HourlySpeed>> HourlyAggregationsForSegmentInTimePeriod(List<Guid> segmentIds, DateTime startTime, DateTime endTime)
        {
            var convertedStartDate = DateOnly.FromDateTime(startTime);
            var convertedEndDate = DateOnly.FromDateTime(endTime);
            TimeOnly convertedStartTime = TimeOnly.FromDateTime(startTime);
            TimeOnly convertedEndTime = TimeOnly.FromDateTime(endTime);
            // Construct a comma-separated list of IDs for the IN clause
            string ids = string.Join(",", segmentIds.Select(id => $"'{id}'"));
            //TIMESTAMP('{item.BinStartTime:yyyy-MM-dd HH:mm:ss}')
            var query = $@"
            SELECT * FROM `{_datasetId}.{_tableId}` 
                WHERE 
                Date BETWEEN @startDate AND @endDate AND
                BinStartTime BETWEEN @startTime AND @endTime
                AND SegmentId IN ({ids})";

            var parameters = new List<BigQueryParameter>();
            parameters.AddRange(new BigQueryParameter[]
                {
                    new BigQueryParameter("startDate", BigQueryDbType.Date, convertedStartDate.ToDateTime(new TimeOnly(0, 0))),
                    new BigQueryParameter("endDate", BigQueryDbType.Date, convertedEndDate.ToDateTime(new TimeOnly(0, 0))),
                    new BigQueryParameter("startTime", BigQueryDbType.Time, convertedStartTime.ToTimeSpan()),
                    new BigQueryParameter("endTime", BigQueryDbType.Time, convertedEndTime.ToTimeSpan())
                });

            var result = await _client.ExecuteQueryAsync(query, parameters);
            var hourlyAggregations = new List<HourlySpeed>();
            foreach (var row in result)
            {
                hourlyAggregations.Add(MapRowToEntity(row));
            }

            return hourlyAggregations;
        }



        #endregion

        #region IApproachRepository

        #endregion
    }
}