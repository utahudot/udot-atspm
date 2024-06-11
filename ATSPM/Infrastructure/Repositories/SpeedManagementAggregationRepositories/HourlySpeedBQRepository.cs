using ATSPM.Application.Business.RouteSpeed;
using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Domain.Extensions;
using Google.Api;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
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

        public async Task<List<DailyAverage>> GetDailyAveragesAsync(int routeId, DateOnly startDate, DateOnly endDate, string daysOfWeek, int sourceId)
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
                SourceId = @sourceId AND
                date BETWEEN @startDate AND @endDate AND
                EXTRACT(DAYOFWEEK FROM date) IN ({daysOfWeek})
            GROUP BY Date
            ORDER BY Date ASC;";

            var parameters = new[]
            {
            new BigQueryParameter("routeId", BigQueryDbType.Int64, routeId),
            new BigQueryParameter("startDate", BigQueryDbType.Date, startDate.ToDateTime(new TimeOnly(0,0))),
            new BigQueryParameter("endDate", BigQueryDbType.Date, endDate.ToDateTime(new TimeOnly(0, 0))),
            new BigQueryParameter("sourceId", BigQueryDbType.Int64, sourceId)
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
                                    `atspm-406601.speed_dataset.route` AS r
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

                //var tempRoutes = Enumerable.Range(0, 14000).Select(x => x.ToString()).ToList(); //bigQuery .Routes.ToList();
                //var routes = new Dictionary<string, int>();
                //foreach (var route in tempRoutes)
                //{
                //    routes.Add(route, 45);
                //}

                List<RouteSpeed> results = new List<RouteSpeed>();
                foreach (BigQueryRow row in queryResults)
                {
                    // Access row data as needed
                    var routeId = row["RouteId"];
                    var avg = row["Avg"];
                    var percentile15 = row["Percentilespd_15"];
                    var percentile85 = row["Percentilespd_85"];
                    var percentile95 = row["Percentilespd_95"];
                    var flow = row["Flow"];
                    var estimatedViolations = row["EstimatedViolations"];
                    var speedLimit = row["SpeedLimit"];
                    // Further processing...

                    var result = new RouteSpeed
                     {
                         RouteId = routeId.ToString(),
                         Name = routeId.ToString(),
                         Avg = Math.Round((double)avg, 2),
                         Percentilespd_15 = (long)percentile15,
                         Percentilespd_85 = (long)percentile85,
                         Percentilespd_95 = (long)percentile95,
                         Flow = (long)flow,
                        EstimatedViolations = (long)estimatedViolations,
                        SpeedLimit = (long)speedLimit
                    };
                    results.Add(result);

                }

                //var results = queryResults
                //.GroupBy(row => row["RouteId"].ToString())
                //.Select(g =>
                //{
                //    // Look into each hour violation find its value using the formula mentioned and then average that out
                //    var speedLimit = routes[g.Key];
                //    long summedFlows = 0;
                //    var summedViolations = g.Sum(row =>
                //    {
                //        long? fifteenthSpeed = row["FifteenthSpeed"] as long?;
                //        long average = (long)row["Average"];
                //        long? eightyFifthSpeed = row["EightyFifthSpeed"] as long?;
                //        long? ninetyFifthSpeed = row["NinetyFifthSpeed"] as long?;
                //        long? flow = row["Flow"] as long?;

                //        if (speedLimit <= 0)
                //        {
                //            return 0;
                //        }
                //        if (fifteenthSpeed.HasValue && HigherThanViolationThreshold(options.ViolationThreshold, fifteenthSpeed.Value, speedLimit))
                //        {
                //            if (flow.HasValue)
                //            {
                //                summedFlows += flow.Value;
                //            }
                //            return flow.HasValue ? (long)(flow * 0.85) * flow : null;
                //        }
                //        else if (HigherThanViolationThreshold(options.ViolationThreshold, average, speedLimit))
                //        {
                //            if (flow.HasValue)
                //            {
                //                summedFlows += flow.Value;
                //            }
                //            return flow.HasValue ? (long)(flow * 0.5) * flow : null;
                //        }
                //        else if (eightyFifthSpeed.HasValue && HigherThanViolationThreshold(options.ViolationThreshold, eightyFifthSpeed.Value, speedLimit))
                //        {
                //            if (flow.HasValue)
                //            {
                //                summedFlows += flow.Value;
                //            }
                //            return flow.HasValue ? (long)(flow * 0.15) * flow : null;
                //        }
                //        else if (ninetyFifthSpeed.HasValue && HigherThanViolationThreshold(options.ViolationThreshold, ninetyFifthSpeed.Value, speedLimit))
                //        {
                //            if (flow.HasValue)
                //            {
                //                summedFlows += flow.Value;
                //            }
                //            return flow.HasValue ? (long)(flow * 0.05) * flow : null;
                //        }
                //        return 0;
                //    });

                //    return new RouteSpeed
                //    {
                //        RouteId = g.Key,
                //        Name = g.Key,
                //        Avg = (long)Math.Round(g.Average(row => (long)row["Average"])),
                //        Percentilespd_15 = GetPercentileSpeed_15(g),
                //        Percentilespd_85 = GetPercentileSpeed_85(g),
                //        Percentilespd_95 = GetPercentileSpeed_95(g),
                //        Flow = g.Sum(row => (long?)row["Flow"]),
                //        EstimatedViolations = summedViolations > 0 ? Convert.ToInt64(Math.Round((double)summedViolations / summedFlows)) : null,
                //        SpeedLimit = speedLimit
                //    };
                //})
                //.ToList();

                return results;
            }
            catch (Exception ex)
            {
                //logger.LogError(ex.Message, $"Error getting route speeds {_dbContext.Database.GetConnectionString()}");
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
            throw new NotImplementedException();
        }

        public override HourlySpeed Lookup(object key)
        {
            throw new NotImplementedException();
        }

        public override HourlySpeed Lookup(HourlySpeed item)
        {
            throw new NotImplementedException();
        }

        public override Task<HourlySpeed> LookupAsync(object key)
        {
            throw new NotImplementedException();
        }

        public override Task<HourlySpeed> LookupAsync(HourlySpeed item)
        {
            throw new NotImplementedException();
        }

        public override void Remove(HourlySpeed item)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRange(IEnumerable<HourlySpeed> items)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveRangeAsync(IEnumerable<HourlySpeed> items)
        {
            throw new NotImplementedException();
        }

        public override void Update(HourlySpeed item)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateAsync(HourlySpeed item)
        {
            throw new NotImplementedException();
        }

        public override void UpdateRange(IEnumerable<HourlySpeed> items)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateRangeAsync(IEnumerable<HourlySpeed> items)
        {
            throw new NotImplementedException();
        }



        #endregion

        #region IApproachRepository

        #endregion
    }
}