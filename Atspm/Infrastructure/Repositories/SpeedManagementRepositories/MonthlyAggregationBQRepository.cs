using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using System.Text;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories
{
    ///<inheritdoc cref="ISegmentRepository"/>
    public class MonthlyAggregationBQRepository : ATSPMRepositoryBQBase<MonthlyAggregation>, IMonthlyAggregationRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<MonthlyAggregation>> _logger;

        public MonthlyAggregationBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<MonthlyAggregation>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
        }

        public override IQueryable<MonthlyAggregation> GetList()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = _client.ExecuteQuery(query, parameters).ToList();

            // Map the result to a list of ImpactType objects
            return result.Select(row => MapRowToEntity(row)).ToList().AsQueryable();
        }

        public override MonthlyAggregation Lookup(object key)
        {
            if (key == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key.ToString())
                };
            var results = _client.ExecuteQuery(query, parameters);
            Task<MonthlyAggregation> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override MonthlyAggregation Lookup(MonthlyAggregation item)
        {
            if (item.Id == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
                };

            var results = _client.ExecuteQuery(query, parameters);
            Task<MonthlyAggregation> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override async Task<MonthlyAggregation> LookupAsync(object key)
        {
            if (key == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key.ToString())
                };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override async Task<MonthlyAggregation> LookupAsync(MonthlyAggregation item)
        {
            if (item.Id == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
                };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override void Remove(MonthlyAggregation item)
        {
            if (item.Id == null) return;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
             };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override async Task RemoveAsync(MonthlyAggregation item)
        {
            if (item.Id == null) return;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
             };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<MonthlyAggregation> items)
        {
            var ids = string.Join(", ", items.Select(i => i.Id));
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("ids", BigQueryDbType.String, ids)
                };

            _client.ExecuteQuery(query, parameters);
        }

        public override async Task RemoveRangeAsync(IEnumerable<MonthlyAggregation> items)
        {
            var ids = string.Join(", ", items.Select(i => i.Id));
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("ids", BigQueryDbType.String, ids)
                };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override async void Update(MonthlyAggregation item)
        {
            var oldRow = await CheckExistanceAsync(item);
            if (oldRow != null)
            {
                string query = updateQuery(item);

                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQuery(query, parameters);
            }
            else
            {
                string query = InsertQueryStatement(item);

                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQuery(query, parameters);
            }

        }
        /// <inheritdoc/>

        public async Task UpsertMonthlyAggregationAsync(MonthlyAggregation item)
        {
            var oldRow = await CheckExistanceAsync(item);
            if (oldRow != null)
            {
                string query = updateQuery(item);

                var parameters = new List<BigQueryParameter>();

                var result = await _client.ExecuteQueryAsync(query, parameters);
                return;
            }
            else
            {
                string query = InsertQueryStatement(item);

                var parameters = new List<BigQueryParameter>();

                var result = await _client.ExecuteQueryAsync(query, parameters);
                return;
            }
        }


        public override async Task UpdateAsync(MonthlyAggregation item)
        {
            var oldRow = await CheckExistanceAsync(item);
            if (oldRow != null)
            {
                string query = updateQuery(item);

                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
            }
            else
            {
                string query = InsertQueryStatement(item);

                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
            }
        }

        public override void UpdateRange(IEnumerable<MonthlyAggregation> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public override async Task UpdateRangeAsync(IEnumerable<MonthlyAggregation> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }

        public async Task<MonthlyAggregation> CheckExistanceAsync(MonthlyAggregation item)
        {
            if (item == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE BinStartTime = TIMESTAMP('{item.BinStartTime:yyyy-MM-dd HH:mm:ss}') AND SegmentId = '{item.SegmentId}' AND SourceId = {item.SourceId}";
            var parameters = new List<BigQueryParameter>();
            var results = await _client.ExecuteQueryAsync(query, parameters);
            Task<MonthlyAggregation> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }



        /// <inheritdoc/>

        public async Task<MonthlyAggregation> SelectByBinTimeSegmentAndSource(DateTime binStartTime, MonthlyAggregation monthlyAggregation)
        {
            var segmentId = monthlyAggregation.SegmentId;
            var sourceId = monthlyAggregation.SourceId;
            var query = $@"
            SELECT *
            FROM `{_datasetId}.{_tableId}`
            WHERE BinStartTime = @binStartTime
              AND SegmentId = @segmentId
              AND SourceId = @sourceId";

            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("binStartTime", BigQueryDbType.DateTime, binStartTime),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString()),
                new BigQueryParameter("sourceId", BigQueryDbType.Int64, sourceId)
            };

            var result = await _client.ExecuteQueryAsync(query, parameters);
            return MapRowToEntity(result.FirstOrDefault());
        }
        /// <inheritdoc/>

        public async Task<MonthlyAggregation> SelectByBinTimeSegment(DateTime binStartTime, MonthlyAggregation monthlyAggregation)
        {
            var segmentId = monthlyAggregation.SegmentId;
            var query = $@"
            SELECT *
            FROM `{_datasetId}.{_tableId}`
            WHERE BinStartTime = @binStartTime
              AND SegmentId = @segmentId";

            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("binStartTime", BigQueryDbType.DateTime, binStartTime),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            var result = await _client.ExecuteQueryAsync(query, parameters);
            return MapRowToEntity(result.FirstOrDefault());
        }
        /// <inheritdoc/>


        public async Task<List<MonthlyAggregation>> SelectMonthlyAggregationBySegment(Guid segmentId)
        {
            var query = $@"
            SELECT *
            FROM `{_datasetId}.{_tableId}`
            WHERE SegmentId = @segmentId";

            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            var result = await _client.ExecuteQueryAsync(query, parameters);
            var monthlyAggregations = new List<MonthlyAggregation>();
            foreach (var row in result)
            {
                monthlyAggregations.Add(MapRowToEntity(row));
            }

            return monthlyAggregations;
        }

        public async Task<List<MonthlyAggregation>> MonthlyAggregationsForSegmentInTimePeriod(List<Guid> segmentIds, DateTime startTime, DateTime endTime)
        {
            // Construct a comma-separated list of IDs for the IN clause
            string ids = string.Join(",", segmentIds.Select(id => $"'{id}'"));
            //TIMESTAMP('{item.BinStartTime:yyyy-MM-dd HH:mm:ss}')
            var query = $@"
            SELECT * FROM `{_datasetId}.{_tableId}` 
                WHERE BinStartTime BETWEEN TIMESTAMP('{startTime:yyyy-MM-dd HH:mm:ss}') AND TIMESTAMP('{endTime:yyyy-MM-dd HH:mm:ss}') 
                AND SegmentId IN ({ids})";

            var parameters = new List<BigQueryParameter>();

            var result = await _client.ExecuteQueryAsync(query, parameters);
            var monthlyAggregations = new List<MonthlyAggregation>();
            foreach (var row in result)
            {
                monthlyAggregations.Add(MapRowToEntity(row));
            }

            return monthlyAggregations;
        }
        /// <inheritdoc/>

        public async Task<List<MonthlyAggregation>> SelectBinStartTimesInRangeFromSource(DateTime startTime, DateTime endTime, MonthlyAggregation monthlyAggregation)
        {
            var segmentId = monthlyAggregation.SegmentId;
            var sourceId = monthlyAggregation.SourceId;
            var query = $@"
                SELECT *
                FROM `{_datasetId}.{_tableId}`
                WHERE BinStartTime BETWEEN @startTime AND @endTime
                  AND SegmentId = @segmentId
                  AND SourceId = @sourceId";

            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("startTime", BigQueryDbType.DateTime, startTime),
                new BigQueryParameter("endTime", BigQueryDbType.DateTime, endTime),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString()),
                new BigQueryParameter("sourceId", BigQueryDbType.Int64, sourceId)
            };

            var result = await _client.ExecuteQueryAsync(query, parameters);
            var monthlyAggregations = new List<MonthlyAggregation>();
            foreach (var row in result)
            {
                monthlyAggregations.Add(MapRowToEntity(row));
            }

            return monthlyAggregations;
        }
        /// <inheritdoc/>

        public async Task<List<MonthlyAggregation>> SelectBinStartTimesInRange(DateTime startTime, DateTime endTime, MonthlyAggregation monthlyAggregation)
        {
            var segmentId = monthlyAggregation.SegmentId;
            var query = $@"
                SELECT *
                FROM `{_datasetId}.{_tableId}`
                WHERE BinStartTime BETWEEN @startTime AND @endTime
                  AND SegmentId = @segmentId";

            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("startTime", BigQueryDbType.DateTime, startTime),
                new BigQueryParameter("endTime", BigQueryDbType.DateTime, endTime),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            var result = await _client.ExecuteQueryAsync(query, parameters);
            var monthlyAggregations = new List<MonthlyAggregation>();
            foreach (var row in result)
            {
                monthlyAggregations.Add(MapRowToEntity(row));
            }

            return monthlyAggregations;
        }

        public async Task<List<MonthlyAggregation>> AllAggregationsOverTimePeriod()
        {
            var thresholdDate = DateTime.UtcNow.AddYears(-2).AddMonths(-1);

            var query = $@"
                SELECT *
                FROM `{_datasetId}.{_tableId}`
                WHERE BinStartTime < @thresholdDate";

            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("thresholdDate", BigQueryDbType.Timestamp, thresholdDate.ToUniversalTime())
                };

            var result = await _client.ExecuteQueryAsync(query, parameters);
            var monthlyAggregations = new List<MonthlyAggregation>();
            foreach (var row in result)
            {
                monthlyAggregations.Add(MapRowToEntity(row));
            }

            return monthlyAggregations;
        }


        protected override BigQueryInsertRow CreateRow(MonthlyAggregation item)
        {
            return new BigQueryInsertRow
            {
                { "Id", item.Id.ToString() },
                { "CreatedDate", item.CreatedDate.Value },
                { "BinStartTime", item.BinStartTime },
                { "SegmentId", item.SegmentId.ToString() },
                { "SourceId", item.SourceId },

                { "AllDayAverageSpeed", item.AllDayAverageSpeed },
                { "AllDayAverageEightyFifthSpeed", item.AllDayAverageEightyFifthSpeed },
                { "AllDayViolations", item.AllDayViolations },
                { "AllDayExtremeViolations", item.AllDayExtremeViolations },
                { "AllDayFlow", item.AllDayFlow },
                { "AllDayMinSpeed", item.AllDayMinSpeed },
                { "AllDayMaxSpeed", item.AllDayMaxSpeed },
                { "AllDayVariability", item.AllDayVariability },
                { "AllDayPercentViolations", item.AllDayPercentViolations },
                { "AllDayPercentExtremeViolations", item.AllDayPercentExtremeViolations },
                { "AllDayAvgSpeedVsSpeedLimit", item.AllDayAvgSpeedVsSpeedLimit },
                { "AllDayEightyFifthSpeedVsSpeedLimit", item.AllDayEightyFifthSpeedVsSpeedLimit },
                { "AllDayPercentObserved", item.AllDayPercentObserved },

                { "WeekendAllDayAverageSpeed", item.WeekendAllDayAverageSpeed },
                { "WeekendAllDayAverageEightyFifthSpeed", item.WeekendAllDayAverageEightyFifthSpeed },
                { "WeekendAllDayViolations", item.WeekendAllDayViolations },
                { "WeekendAllDayExtremeViolations", item.WeekendAllDayExtremeViolations },
                { "WeekendAllDayFlow", item.WeekendAllDayFlow },
                { "WeekendAllDayMinSpeed", item.WeekendAllDayMinSpeed },
                { "WeekendAllDayMaxSpeed", item.WeekendAllDayMaxSpeed },
                { "WeekendAllDayVariability", item.WeekendAllDayVariability },
                { "WeekendAllDayPercentViolations", item.WeekendAllDayPercentViolations },
                { "WeekendAllDayPercentExtremeViolations", item.WeekendAllDayPercentExtremeViolations },
                { "WeekendAllDayAvgSpeedVsSpeedLimit", item.WeekendAllDayAvgSpeedVsSpeedLimit },
                { "WeekendAllDayEightyFifthSpeedVsSpeedLimit", item.WeekendAllDayEightyFifthSpeedVsSpeedLimit },
                { "WeekendAllDayPercentObserved", item.WeekendAllDayPercentObserved },

                { "WeekdayAllDayAverageSpeed", item.WeekdayAllDayAverageSpeed },
                { "WeekdayAllDayAverageEightyFifthSpeed", item.WeekdayAllDayAverageEightyFifthSpeed },
                { "WeekdayAllDayViolations", item.WeekdayAllDayViolations },
                { "WeekdayAllDayExtremeViolations", item.WeekdayAllDayExtremeViolations },
                { "WeekdayAllDayFlow", item.WeekdayAllDayFlow },
                { "WeekdayAllDayMinSpeed", item.WeekdayAllDayMinSpeed },
                { "WeekdayAllDayMaxSpeed", item.WeekdayAllDayMaxSpeed },
                { "WeekdayAllDayVariability", item.WeekdayAllDayVariability },
                { "WeekdayAllDayPercentViolations", item.WeekdayAllDayPercentViolations },
                { "WeekdayAllDayPercentExtremeViolations", item.WeekdayAllDayPercentExtremeViolations },
                { "WeekdayAllDayAvgSpeedVsSpeedLimit", item.WeekdayAllDayAvgSpeedVsSpeedLimit },
                { "WeekdayAllDayEightyFifthSpeedVsSpeedLimit", item.WeekdayAllDayEightyFifthSpeedVsSpeedLimit },
                { "WeekdayAllDayPercentObserved", item.WeekdayAllDayPercentObserved },

                { "OffPeakAverageSpeed", item.OffPeakAverageSpeed },
                { "OffPeakAverageEightyFifthSpeed", item.OffPeakAverageEightyFifthSpeed },
                { "OffPeakViolations", item.OffPeakViolations },
                { "OffPeakExtremeViolations", item.OffPeakExtremeViolations },
                { "OffPeakFlow", item.OffPeakFlow },
                { "OffPeakMinSpeed", item.OffPeakMinSpeed },
                { "OffPeakMaxSpeed", item.OffPeakMaxSpeed },
                { "OffPeakVariability", item.OffPeakVariability },
                { "OffPeakPercentViolations", item.OffPeakPercentViolations },
                { "OffPeakPercentExtremeViolations", item.OffPeakPercentExtremeViolations },
                { "OffPeakAvgSpeedVsSpeedLimit", item.OffPeakAvgSpeedVsSpeedLimit },
                { "OffPeakEightyFifthSpeedVsSpeedLimit", item.OffPeakEightyFifthSpeedVsSpeedLimit },
                { "OffPeakPercentObserved", item.OffPeakPercentObserved },

                { "WeekendOffPeakAverageSpeed", item.WeekendOffPeakAverageSpeed },
                { "WeekendOffPeakAverageEightyFifthSpeed", item.WeekendOffPeakAverageEightyFifthSpeed },
                { "WeekendOffPeakViolations", item.WeekendOffPeakViolations },
                { "WeekendOffPeakExtremeViolations", item.WeekendOffPeakExtremeViolations },
                { "WeekendOffPeakFlow", item.WeekendOffPeakFlow },
                { "WeekendOffPeakMinSpeed", item.WeekendOffPeakMinSpeed },
                { "WeekendOffPeakMaxSpeed", item.WeekendOffPeakMaxSpeed },
                { "WeekendOffPeakVariability", item.WeekendOffPeakVariability },
                { "WeekendOffPeakPercentViolations", item.WeekendOffPeakPercentViolations },
                { "WeekendOffPeakPercentExtremeViolations", item.WeekendOffPeakPercentExtremeViolations },
                { "WeekendOffPeakAvgSpeedVsSpeedLimit", item.WeekendOffPeakAvgSpeedVsSpeedLimit },
                { "WeekendOffPeakEightyFifthSpeedVsSpeedLimit", item.WeekendOffPeakEightyFifthSpeedVsSpeedLimit },
                { "WeekendOffPeakPercentObserved", item.WeekendOffPeakPercentObserved },

                { "WeekdayOffPeakAverageSpeed", item.WeekdayOffPeakAverageSpeed },
                { "WeekdayOffPeakAverageEightyFifthSpeed", item.WeekdayOffPeakAverageEightyFifthSpeed },
                { "WeekdayOffPeakViolations", item.WeekdayOffPeakViolations },
                { "WeekdayOffPeakExtremeViolations", item.WeekdayOffPeakExtremeViolations },
                { "WeekdayOffPeakFlow", item.WeekdayOffPeakFlow },
                { "WeekdayOffPeakMinSpeed", item.WeekdayOffPeakMinSpeed },
                { "WeekdayOffPeakMaxSpeed", item.WeekdayOffPeakMaxSpeed },
                { "WeekdayOffPeakVariability", item.WeekdayOffPeakVariability },
                { "WeekdayOffPeakPercentViolations", item.WeekdayOffPeakPercentViolations },
                { "WeekdayOffPeakPercentExtremeViolations", item.WeekdayOffPeakPercentExtremeViolations },
                { "WeekdayOffPeakAvgSpeedVsSpeedLimit", item.WeekdayOffPeakAvgSpeedVsSpeedLimit },
                { "WeekdayOffPeakEightyFifthSpeedVsSpeedLimit", item.WeekdayOffPeakEightyFifthSpeedVsSpeedLimit },
                { "WeekdayOffPeakPercentObserved", item.WeekdayOffPeakPercentObserved },

                { "AmPeakAverageSpeed", item.AmPeakAverageSpeed },
                { "AmPeakAverageEightyFifthSpeed", item.AmPeakAverageEightyFifthSpeed },
                { "AmPeakViolations", item.AmPeakViolations },
                { "AmPeakExtremeViolations", item.AmPeakExtremeViolations },
                { "AmPeakFlow", item.AmPeakFlow },
                { "AmPeakMinSpeed", item.AmPeakMinSpeed },
                { "AmPeakMaxSpeed", item.AmPeakMaxSpeed },
                { "AmPeakVariability", item.AmPeakVariability },
                { "AmPeakPercentViolations", item.AmPeakPercentViolations },
                { "AmPeakPercentExtremeViolations", item.AmPeakPercentExtremeViolations },
                { "AmPeakAvgSpeedVsSpeedLimit", item.AmPeakAvgSpeedVsSpeedLimit },
                { "AmPeakEightyFifthSpeedVsSpeedLimit", item.AmPeakEightyFifthSpeedVsSpeedLimit },
                { "AmPeakPercentObserved", item.AmPeakPercentObserved },

                { "WeekendAmPeakAverageSpeed", item.WeekendAmPeakAverageSpeed },
                { "WeekendAmPeakAverageEightyFifthSpeed", item.WeekendAmPeakAverageEightyFifthSpeed },
                { "WeekendAmPeakViolations", item.WeekendAmPeakViolations },
                { "WeekendAmPeakExtremeViolations", item.WeekendAmPeakExtremeViolations },
                { "WeekendAmPeakFlow", item.WeekendAmPeakFlow },
                { "WeekendAmPeakMinSpeed", item.WeekendAmPeakMinSpeed },
                { "WeekendAmPeakMaxSpeed", item.WeekendAmPeakMaxSpeed },
                { "WeekendAmPeakVariability", item.WeekendAmPeakVariability },
                { "WeekendAmPeakPercentViolations", item.WeekendAmPeakPercentViolations },
                { "WeekendAmPeakPercentExtremeViolations", item.WeekendAmPeakPercentExtremeViolations },
                { "WeekendAmPeakAvgSpeedVsSpeedLimit", item.WeekendAmPeakAvgSpeedVsSpeedLimit },
                { "WeekendAmPeakEightyFifthSpeedVsSpeedLimit", item.WeekendAmPeakEightyFifthSpeedVsSpeedLimit },
                { "WeekendAmPeakPercentObserved", item.WeekendAmPeakPercentObserved },

                { "WeekdayAmPeakAverageSpeed", item.WeekdayAmPeakAverageSpeed },
                { "WeekdayAmPeakAverageEightyFifthSpeed", item.WeekdayAmPeakAverageEightyFifthSpeed },
                { "WeekdayAmPeakViolations", item.WeekdayAmPeakViolations },
                { "WeekdayAmPeakExtremeViolations", item.WeekdayAmPeakExtremeViolations },
                { "WeekdayAmPeakFlow", item.WeekdayAmPeakFlow },
                { "WeekdayAmPeakMinSpeed", item.WeekdayAmPeakMinSpeed },
                { "WeekdayAmPeakMaxSpeed", item.WeekdayAmPeakMaxSpeed },
                { "WeekdayAmPeakVariability", item.WeekdayAmPeakVariability },
                { "WeekdayAmPeakPercentViolations", item.WeekdayAmPeakPercentViolations },
                { "WeekdayAmPeakPercentExtremeViolations", item.WeekdayAmPeakPercentExtremeViolations },
                { "WeekdayAmPeakAvgSpeedVsSpeedLimit", item.WeekdayAmPeakAvgSpeedVsSpeedLimit },
                { "WeekdayAmPeakEightyFifthSpeedVsSpeedLimit", item.WeekdayAmPeakEightyFifthSpeedVsSpeedLimit },
                { "WeekdayAmPeakPercentObserved", item.WeekdayAmPeakPercentObserved },

                { "PmPeakAverageSpeed", item.PmPeakAverageSpeed },
                { "PmPeakAverageEightyFifthSpeed", item.PmPeakAverageEightyFifthSpeed },
                { "PmPeakViolations", item.PmPeakViolations },
                { "PmPeakExtremeViolations", item.PmPeakExtremeViolations },
                { "PmPeakFlow", item.PmPeakFlow },
                { "PmPeakMinSpeed", item.PmPeakMinSpeed },
                { "PmPeakMaxSpeed", item.PmPeakMaxSpeed },
                { "PmPeakVariability", item.PmPeakVariability },
                { "PmPeakPercentViolations", item.PmPeakPercentViolations },
                { "PmPeakPercentExtremeViolations", item.PmPeakPercentExtremeViolations },
                { "PmPeakAvgSpeedVsSpeedLimit", item.PmPeakAvgSpeedVsSpeedLimit },
                { "PmPeakEightyFifthSpeedVsSpeedLimit", item.PmPeakEightyFifthSpeedVsSpeedLimit },
                { "PmPeakPercentObserved", item.PmPeakPercentObserved },

                { "WeekendPmPeakAverageSpeed", item.WeekendPmPeakAverageSpeed },
                { "WeekendPmPeakAverageEightyFifthSpeed", item.WeekendPmPeakAverageEightyFifthSpeed },
                { "WeekendPmPeakViolations", item.WeekendPmPeakViolations },
                { "WeekendPmPeakExtremeViolations", item.WeekendPmPeakExtremeViolations },
                { "WeekendPmPeakFlow", item.WeekendPmPeakFlow },
                { "WeekendPmPeakMinSpeed", item.WeekendPmPeakMinSpeed },
                { "WeekendPmPeakMaxSpeed", item.WeekendPmPeakMaxSpeed },
                { "WeekendPmPeakVariability", item.WeekendPmPeakVariability },
                { "WeekendPmPeakPercentViolations", item.WeekendPmPeakPercentViolations },
                { "WeekendPmPeakPercentExtremeViolations", item.WeekendPmPeakPercentExtremeViolations },
                { "WeekendPmPeakAvgSpeedVsSpeedLimit", item.WeekendPmPeakAvgSpeedVsSpeedLimit },
                { "WeekendPmPeakEightyFifthSpeedVsSpeedLimit", item.WeekendPmPeakEightyFifthSpeedVsSpeedLimit },
                { "WeekendPmPeakPercentObserved", item.WeekendPmPeakPercentObserved },

                { "WeekdayPmPeakAverageSpeed", item.WeekdayPmPeakAverageSpeed },
                { "WeekdayPmPeakAverageEightyFifthSpeed", item.WeekdayPmPeakAverageEightyFifthSpeed },
                { "WeekdayPmPeakViolations", item.WeekdayPmPeakViolations },
                { "WeekdayPmPeakExtremeViolations", item.WeekdayPmPeakExtremeViolations },
                { "WeekdayPmPeakFlow", item.WeekdayPmPeakFlow },
                { "WeekdayPmPeakMinSpeed", item.WeekdayPmPeakMinSpeed },
                { "WeekdayPmPeakMaxSpeed", item.WeekdayPmPeakMaxSpeed },
                { "WeekdayPmPeakVariability", item.WeekdayPmPeakVariability },
                { "WeekdayPmPeakPercentViolations", item.WeekdayPmPeakPercentViolations },
                { "WeekdayPmPeakPercentExtremeViolations", item.WeekdayPmPeakPercentExtremeViolations },
                { "WeekdayPmPeakAvgSpeedVsSpeedLimit", item.WeekdayPmPeakAvgSpeedVsSpeedLimit },
                { "WeekdayPmPeakEightyFifthSpeedVsSpeedLimit", item.WeekdayPmPeakEightyFifthSpeedVsSpeedLimit },
                { "WeekdayPmPeakPercentObserved", item.WeekdayPmPeakPercentObserved },

                { "MidDayAverageSpeed", item.MidDayAverageSpeed },
                { "MidDayAverageEightyFifthSpeed", item.MidDayAverageEightyFifthSpeed },
                { "MidDayViolations", item.MidDayViolations },
                { "MidDayExtremeViolations", item.MidDayExtremeViolations },
                { "MidDayFlow", item.MidDayFlow },
                { "MidDayMinSpeed", item.MidDayMinSpeed },
                { "MidDayMaxSpeed", item.MidDayMaxSpeed },
                { "MidDayVariability", item.MidDayVariability },
                { "MidDayPercentViolations", item.MidDayPercentViolations },
                { "MidDayPercentExtremeViolations", item.MidDayPercentExtremeViolations },
                { "MidDayAvgSpeedVsSpeedLimit", item.MidDayAvgSpeedVsSpeedLimit },
                { "MidDayEightyFifthSpeedVsSpeedLimit", item.MidDayEightyFifthSpeedVsSpeedLimit },
                { "MidDayPercentObserved", item.MidDayPercentObserved },

                { "WeekendMidDayAverageSpeed", item.WeekendMidDayAverageSpeed },
                { "WeekendMidDayAverageEightyFifthSpeed", item.WeekendMidDayAverageEightyFifthSpeed },
                { "WeekendMidDayViolations", item.WeekendMidDayViolations },
                { "WeekendMidDayExtremeViolations", item.WeekendMidDayExtremeViolations },
                { "WeekendMidDayFlow", item.WeekendMidDayFlow },
                { "WeekendMidDayMinSpeed", item.WeekendMidDayMinSpeed },
                { "WeekendMidDayMaxSpeed", item.WeekendMidDayMaxSpeed },
                { "WeekendMidDayVariability", item.WeekendMidDayVariability },
                { "WeekendMidDayPercentViolations", item.WeekendMidDayPercentViolations },
                { "WeekendMidDayPercentExtremeViolations", item.WeekendMidDayPercentExtremeViolations },
                { "WeekendMidDayAvgSpeedVsSpeedLimit", item.WeekendMidDayAvgSpeedVsSpeedLimit },
                { "WeekendMidDayEightyFifthSpeedVsSpeedLimit", item.WeekendMidDayEightyFifthSpeedVsSpeedLimit },
                { "WeekendMidDayPercentObserved", item.WeekendMidDayPercentObserved },

                { "WeekdayMidDayAverageSpeed", item.WeekdayMidDayAverageSpeed },
                { "WeekdayMidDayAverageEightyFifthSpeed", item.WeekdayMidDayAverageEightyFifthSpeed },
                { "WeekdayMidDayViolations", item.WeekdayMidDayViolations },
                { "WeekdayMidDayExtremeViolations", item.WeekdayMidDayExtremeViolations },
                { "WeekdayMidDayFlow", item.WeekdayMidDayFlow },
                { "WeekdayMidDayMinSpeed", item.WeekdayMidDayMinSpeed },
                { "WeekdayMidDayMaxSpeed", item.WeekdayMidDayMaxSpeed },
                { "WeekdayMidDayVariability", item.WeekdayMidDayVariability },
                { "WeekdayMidDayPercentViolations", item.WeekdayMidDayPercentViolations },
                { "WeekdayMidDayPercentExtremeViolations", item.WeekdayMidDayPercentExtremeViolations },
                { "WeekdayMidDayAvgSpeedVsSpeedLimit", item.WeekdayMidDayAvgSpeedVsSpeedLimit },
                { "WeekdayMidDayEightyFifthSpeedVsSpeedLimit", item.WeekdayMidDayEightyFifthSpeedVsSpeedLimit },
                { "WeekdayMidDayPercentObserved", item.WeekdayMidDayPercentObserved },

                { "EveningAverageSpeed", item.EveningAverageSpeed },
                { "EveningAverageEightyFifthSpeed", item.EveningAverageEightyFifthSpeed },
                { "EveningViolations", item.EveningViolations },
                { "EveningExtremeViolations", item.EveningExtremeViolations },
                { "EveningFlow", item.EveningFlow },
                { "EveningMinSpeed", item.EveningMinSpeed },
                { "EveningMaxSpeed", item.EveningMaxSpeed },
                { "EveningVariability", item.EveningVariability },
                { "EveningPercentViolations", item.EveningPercentViolations },
                { "EveningPercentExtremeViolations", item.EveningPercentExtremeViolations },
                { "EveningAvgSpeedVsSpeedLimit", item.EveningAvgSpeedVsSpeedLimit },
                { "EveningEightyFifthSpeedVsSpeedLimit", item.EveningEightyFifthSpeedVsSpeedLimit },
                { "EveningPercentObserved", item.EveningPercentObserved },

                { "WeekendEveningAverageSpeed", item.WeekendEveningAverageSpeed },
                { "WeekendEveningAverageEightyFifthSpeed", item.WeekendEveningAverageEightyFifthSpeed },
                { "WeekendEveningViolations", item.WeekendEveningViolations },
                { "WeekendEveningExtremeViolations", item.WeekendEveningExtremeViolations },
                { "WeekendEveningFlow", item.WeekendEveningFlow },
                { "WeekendEveningMinSpeed", item.WeekendEveningMinSpeed },
                { "WeekendEveningMaxSpeed", item.WeekendEveningMaxSpeed },
                { "WeekendEveningVariability", item.WeekendEveningVariability },
                { "WeekendEveningPercentViolations", item.WeekendEveningPercentViolations },
                { "WeekendEveningPercentExtremeViolations", item.WeekendEveningPercentExtremeViolations },
                { "WeekendEveningAvgSpeedVsSpeedLimit", item.WeekendEveningAvgSpeedVsSpeedLimit },
                { "WeekendEveningEightyFifthSpeedVsSpeedLimit", item.WeekendEveningEightyFifthSpeedVsSpeedLimit },
                { "WeekendEveningPercentObserved", item.WeekendEveningPercentObserved },

                { "WeekdayEveningAverageSpeed", item.WeekdayEveningAverageSpeed },
                { "WeekdayEveningAverageEightyFifthSpeed", item.WeekdayEveningAverageEightyFifthSpeed },
                { "WeekdayEveningViolations", item.WeekdayEveningViolations },
                { "WeekdayEveningExtremeViolations", item.WeekdayEveningExtremeViolations },
                { "WeekdayEveningFlow", item.WeekdayEveningFlow },
                { "WeekdayEveningMinSpeed", item.WeekdayEveningMinSpeed },
                { "WeekdayEveningMaxSpeed", item.WeekdayEveningMaxSpeed },
                { "WeekdayEveningVariability", item.WeekdayEveningVariability },
                { "WeekdayEveningPercentViolations", item.WeekdayEveningPercentViolations },
                { "WeekdayEveningPercentExtremeViolations", item.WeekdayEveningPercentExtremeViolations },
                { "WeekdayEveningAvgSpeedVsSpeedLimit", item.WeekdayEveningAvgSpeedVsSpeedLimit },
                { "WeekdayEveningEightyFifthSpeedVsSpeedLimit", item.WeekdayEveningEightyFifthSpeedVsSpeedLimit },
                { "WeekdayEveningPercentObserved", item.WeekdayEveningPercentObserved },

                { "EarlyMorningAverageSpeed", item.EarlyMorningAverageSpeed },
                { "EarlyMorningAverageEightyFifthSpeed", item.EarlyMorningAverageEightyFifthSpeed },
                { "EarlyMorningViolations", item.EarlyMorningViolations },
                { "EarlyMorningExtremeViolations", item.EarlyMorningExtremeViolations },
                { "EarlyMorningFlow", item.EarlyMorningFlow },
                { "EarlyMorningMinSpeed", item.EarlyMorningMinSpeed },
                { "EarlyMorningMaxSpeed", item.EarlyMorningMaxSpeed },
                { "EarlyMorningVariability", item.EarlyMorningVariability },
                { "EarlyMorningPercentViolations", item.EarlyMorningPercentViolations },
                { "EarlyMorningPercentExtremeViolations", item.EarlyMorningPercentExtremeViolations },
                { "EarlyMorningAvgSpeedVsSpeedLimit", item.EarlyMorningAvgSpeedVsSpeedLimit },
                { "EarlyMorningEightyFifthSpeedVsSpeedLimit", item.EarlyMorningEightyFifthSpeedVsSpeedLimit },
                { "EarlyMorningPercentObserved", item.EarlyMorningPercentObserved },

                { "WeekendEarlyMorningAverageSpeed", item.WeekendEarlyMorningAverageSpeed },
                { "WeekendEarlyMorningAverageEightyFifthSpeed", item.WeekendEarlyMorningAverageEightyFifthSpeed },
                { "WeekendEarlyMorningViolations", item.WeekendEarlyMorningViolations },
                { "WeekendEarlyMorningExtremeViolations", item.WeekendEarlyMorningExtremeViolations },
                { "WeekendEarlyMorningFlow", item.WeekendEarlyMorningFlow },
                { "WeekendEarlyMorningMinSpeed", item.WeekendEarlyMorningMinSpeed },
                { "WeekendEarlyMorningMaxSpeed", item.WeekendEarlyMorningMaxSpeed },
                { "WeekendEarlyMorningVariability", item.WeekendEarlyMorningVariability },
                { "WeekendEarlyMorningPercentViolations", item.WeekendEarlyMorningPercentViolations },
                { "WeekendEarlyMorningPercentExtremeViolations", item.WeekendEarlyMorningPercentExtremeViolations },
                { "WeekendEarlyMorningAvgSpeedVsSpeedLimit", item.WeekendEarlyMorningAvgSpeedVsSpeedLimit },
                { "WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit", item.WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit },
                { "WeekendEarlyMorningPercentObserved", item.WeekendEarlyMorningPercentObserved },

                { "WeekdayEarlyMorningAverageSpeed", item.WeekdayEarlyMorningAverageSpeed },
                { "WeekdayEarlyMorningAverageEightyFifthSpeed", item.WeekdayEarlyMorningAverageEightyFifthSpeed },
                { "WeekdayEarlyMorningViolations", item.WeekdayEarlyMorningViolations },
                { "WeekdayEarlyMorningExtremeViolations", item.WeekdayEarlyMorningExtremeViolations },
                { "WeekdayEarlyMorningFlow", item.WeekdayEarlyMorningFlow },
                { "WeekdayEarlyMorningMinSpeed", item.WeekdayEarlyMorningMinSpeed },
                { "WeekdayEarlyMorningMaxSpeed", item.WeekdayEarlyMorningMaxSpeed },
                { "WeekdayEarlyMorningVariability", item.WeekdayEarlyMorningVariability },
                { "WeekdayEarlyMorningPercentViolations", item.WeekdayEarlyMorningPercentViolations },
                { "WeekdayEarlyMorningPercentExtremeViolations", item.WeekdayEarlyMorningPercentExtremeViolations },
                { "WeekdayEarlyMorningAvgSpeedVsSpeedLimit", item.WeekdayEarlyMorningAvgSpeedVsSpeedLimit },
                { "WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit", item.WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit },
                { "WeekdayEarlyMorningPercentObserved", item.WeekdayEarlyMorningPercentObserved },

                { "PercentObserved", item.PercentObserved }
            };
        }

        protected override MonthlyAggregation MapRowToEntity(BigQueryRow row)
        {
            var bigQueryId = Guid.Parse(row["Id"].ToString());
            var bigQueryCreatedDate = DateTime.Parse(row["CreatedDate"].ToString());
            var bigQueryBinStartTime = DateTime.Parse(row["BinStartTime"].ToString());
            var bigQuerySegmentId = Guid.Parse(row["SegmentId"].ToString());
            var bigQuerySourceId = int.Parse(row["SourceId"].ToString());

            var bigQueryAllDayAverageSpeed = row["AllDayAverageSpeed"] != null ? double.Parse(row["AllDayAverageSpeed"].ToString()) : (double?)null;
            var bigQueryAllDayAverageEightyFifthSpeed = row["AllDayAverageEightyFifthSpeed"] != null ? double.Parse(row["AllDayAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryAllDayViolations = row["AllDayViolations"] != null ? int.Parse(row["AllDayViolations"].ToString()) : (int?)null;
            var bigQueryAllDayExtremeViolations = row["AllDayExtremeViolations"] != null ? int.Parse(row["AllDayExtremeViolations"].ToString()) : (int?)null;
            var bigQueryAllDayFlow = row["AllDayFlow"] != null ? int.Parse(row["AllDayFlow"].ToString()) : (int?)null;
            var bigQueryAllDayMinSpeed = row["AllDayMinSpeed"] != null ? double.Parse(row["AllDayMinSpeed"].ToString()) : (double?)null;
            var bigQueryAllDayMaxSpeed = row["AllDayMaxSpeed"] != null ? double.Parse(row["AllDayMaxSpeed"].ToString()) : (double?)null;
            var bigQueryAllDayVariability = row["AllDayVariability"] != null ? double.Parse(row["AllDayVariability"].ToString()) : (double?)null;
            var bigQueryAllDayPercentViolations = row["AllDayPercentViolations"] != null ? double.Parse(row["AllDayPercentViolations"].ToString()) : (double?)null;
            var bigQueryAllDayPercentExtremeViolations = row["AllDayPercentExtremeViolations"] != null ? double.Parse(row["AllDayPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryAllDayAvgSpeedVsSpeedLimit = row["AllDayAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["AllDayAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryAllDayEightyFifthSpeedVsSpeedLimit = row["AllDayEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["AllDayEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryAllDayPercentObserved = row["AllDayPercentObserved"] != null ? double.Parse(row["AllDayPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekendAllDayAverageSpeed = row["WeekendAllDayAverageSpeed"] != null ? double.Parse(row["WeekendAllDayAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayAverageEightyFifthSpeed = row["WeekendAllDayAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekendAllDayAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayViolations = row["WeekendAllDayViolations"] != null ? int.Parse(row["WeekendAllDayViolations"].ToString()) : (int?)null;
            var bigQueryWeekendAllDayExtremeViolations = row["WeekendAllDayExtremeViolations"] != null ? int.Parse(row["WeekendAllDayExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekendAllDayFlow = row["WeekendAllDayFlow"] != null ? int.Parse(row["WeekendAllDayFlow"].ToString()) : (int?)null;
            var bigQueryWeekendAllDayMinSpeed = row["WeekendAllDayMinSpeed"] != null ? double.Parse(row["WeekendAllDayMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayMaxSpeed = row["WeekendAllDayMaxSpeed"] != null ? double.Parse(row["WeekendAllDayMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayVariability = row["WeekendAllDayVariability"] != null ? double.Parse(row["WeekendAllDayVariability"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayPercentViolations = row["WeekendAllDayPercentViolations"] != null ? double.Parse(row["WeekendAllDayPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayPercentExtremeViolations = row["WeekendAllDayPercentExtremeViolations"] != null ? double.Parse(row["WeekendAllDayPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayAvgSpeedVsSpeedLimit = row["WeekendAllDayAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendAllDayAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayEightyFifthSpeedVsSpeedLimit = row["WeekendAllDayEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendAllDayEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendAllDayPercentObserved = row["WeekendAllDayPercentObserved"] != null ? double.Parse(row["WeekendAllDayPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekdayAllDayAverageSpeed = row["WeekdayAllDayAverageSpeed"] != null ? double.Parse(row["WeekdayAllDayAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayAverageEightyFifthSpeed = row["WeekdayAllDayAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekdayAllDayAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayViolations = row["WeekdayAllDayViolations"] != null ? int.Parse(row["WeekdayAllDayViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayAllDayExtremeViolations = row["WeekdayAllDayExtremeViolations"] != null ? int.Parse(row["WeekdayAllDayExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayAllDayFlow = row["WeekdayAllDayFlow"] != null ? int.Parse(row["WeekdayAllDayFlow"].ToString()) : (int?)null;
            var bigQueryWeekdayAllDayMinSpeed = row["WeekdayAllDayMinSpeed"] != null ? double.Parse(row["WeekdayAllDayMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayMaxSpeed = row["WeekdayAllDayMaxSpeed"] != null ? double.Parse(row["WeekdayAllDayMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayVariability = row["WeekdayAllDayVariability"] != null ? double.Parse(row["WeekdayAllDayVariability"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayPercentViolations = row["WeekdayAllDayPercentViolations"] != null ? double.Parse(row["WeekdayAllDayPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayPercentExtremeViolations = row["WeekdayAllDayPercentExtremeViolations"] != null ? double.Parse(row["WeekdayAllDayPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayAvgSpeedVsSpeedLimit = row["WeekdayAllDayAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayAllDayAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayEightyFifthSpeedVsSpeedLimit = row["WeekdayAllDayEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayAllDayEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayAllDayPercentObserved = row["WeekdayAllDayPercentObserved"] != null ? double.Parse(row["WeekdayAllDayPercentObserved"].ToString()) : (double?)null;

            var bigQueryOffPeakAverageSpeed = row["OffPeakAverageSpeed"] != null ? double.Parse(row["OffPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryOffPeakAverageEightyFifthSpeed = row["OffPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["OffPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryOffPeakViolations = row["OffPeakViolations"] != null ? int.Parse(row["OffPeakViolations"].ToString()) : (int?)null;
            var bigQueryOffPeakExtremeViolations = row["OffPeakExtremeViolations"] != null ? int.Parse(row["OffPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryOffPeakFlow = row["OffPeakFlow"] != null ? int.Parse(row["OffPeakFlow"].ToString()) : (int?)null;
            var bigQueryOffPeakMinSpeed = row["OffPeakMinSpeed"] != null ? double.Parse(row["OffPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryOffPeakMaxSpeed = row["OffPeakMaxSpeed"] != null ? double.Parse(row["OffPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryOffPeakVariability = row["OffPeakVariability"] != null ? double.Parse(row["OffPeakVariability"].ToString()) : (double?)null;
            var bigQueryOffPeakPercentViolations = row["OffPeakPercentViolations"] != null ? double.Parse(row["OffPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryOffPeakPercentExtremeViolations = row["OffPeakPercentExtremeViolations"] != null ? double.Parse(row["OffPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryOffPeakAvgSpeedVsSpeedLimit = row["OffPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["OffPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryOffPeakEightyFifthSpeedVsSpeedLimit = row["OffPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["OffPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryOffPeakPercentObserved = row["OffPeakPercentObserved"] != null ? double.Parse(row["OffPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekendOffPeakAverageSpeed = row["WeekendOffPeakAverageSpeed"] != null ? double.Parse(row["WeekendOffPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakAverageEightyFifthSpeed = row["WeekendOffPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekendOffPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakViolations = row["WeekendOffPeakViolations"] != null ? int.Parse(row["WeekendOffPeakViolations"].ToString()) : (int?)null;
            var bigQueryWeekendOffPeakExtremeViolations = row["WeekendOffPeakExtremeViolations"] != null ? int.Parse(row["WeekendOffPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekendOffPeakFlow = row["WeekendOffPeakFlow"] != null ? int.Parse(row["WeekendOffPeakFlow"].ToString()) : (int?)null;
            var bigQueryWeekendOffPeakMinSpeed = row["WeekendOffPeakMinSpeed"] != null ? double.Parse(row["WeekendOffPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakMaxSpeed = row["WeekendOffPeakMaxSpeed"] != null ? double.Parse(row["WeekendOffPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakVariability = row["WeekendOffPeakVariability"] != null ? double.Parse(row["WeekendOffPeakVariability"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakPercentViolations = row["WeekendOffPeakPercentViolations"] != null ? double.Parse(row["WeekendOffPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakPercentExtremeViolations = row["WeekendOffPeakPercentExtremeViolations"] != null ? double.Parse(row["WeekendOffPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakAvgSpeedVsSpeedLimit = row["WeekendOffPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendOffPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakEightyFifthSpeedVsSpeedLimit = row["WeekendOffPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendOffPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendOffPeakPercentObserved = row["WeekendOffPeakPercentObserved"] != null ? double.Parse(row["WeekendOffPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekdayOffPeakAverageSpeed = row["WeekdayOffPeakAverageSpeed"] != null ? double.Parse(row["WeekdayOffPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakAverageEightyFifthSpeed = row["WeekdayOffPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekdayOffPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakViolations = row["WeekdayOffPeakViolations"] != null ? int.Parse(row["WeekdayOffPeakViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayOffPeakExtremeViolations = row["WeekdayOffPeakExtremeViolations"] != null ? int.Parse(row["WeekdayOffPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayOffPeakFlow = row["WeekdayOffPeakFlow"] != null ? int.Parse(row["WeekdayOffPeakFlow"].ToString()) : (int?)null;
            var bigQueryWeekdayOffPeakMinSpeed = row["WeekdayOffPeakMinSpeed"] != null ? double.Parse(row["WeekdayOffPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakMaxSpeed = row["WeekdayOffPeakMaxSpeed"] != null ? double.Parse(row["WeekdayOffPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakVariability = row["WeekdayOffPeakVariability"] != null ? double.Parse(row["WeekdayOffPeakVariability"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakPercentViolations = row["WeekdayOffPeakPercentViolations"] != null ? double.Parse(row["WeekdayOffPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakPercentExtremeViolations = row["WeekdayOffPeakPercentExtremeViolations"] != null ? double.Parse(row["WeekdayOffPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakAvgSpeedVsSpeedLimit = row["WeekdayOffPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayOffPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakEightyFifthSpeedVsSpeedLimit = row["WeekdayOffPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayOffPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayOffPeakPercentObserved = row["WeekdayOffPeakPercentObserved"] != null ? double.Parse(row["WeekdayOffPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryAmPeakAverageSpeed = row["AmPeakAverageSpeed"] != null ? double.Parse(row["AmPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryAmPeakAverageEightyFifthSpeed = row["AmPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["AmPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryAmPeakViolations = row["AmPeakViolations"] != null ? int.Parse(row["AmPeakViolations"].ToString()) : (int?)null;
            var bigQueryAmPeakExtremeViolations = row["AmPeakExtremeViolations"] != null ? int.Parse(row["AmPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryAmPeakFlow = row["AmPeakFlow"] != null ? int.Parse(row["AmPeakFlow"].ToString()) : (int?)null;
            var bigQueryAmPeakMinSpeed = row["AmPeakMinSpeed"] != null ? double.Parse(row["AmPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryAmPeakMaxSpeed = row["AmPeakMaxSpeed"] != null ? double.Parse(row["AmPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryAmPeakVariability = row["AmPeakVariability"] != null ? double.Parse(row["AmPeakVariability"].ToString()) : (double?)null;
            var bigQueryAmPeakPercentViolations = row["AmPeakPercentViolations"] != null ? double.Parse(row["AmPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryAmPeakPercentExtremeViolations = row["AmPeakPercentExtremeViolations"] != null ? double.Parse(row["AmPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryAmPeakAvgSpeedVsSpeedLimit = row["AmPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["AmPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryAmPeakEightyFifthSpeedVsSpeedLimit = row["AmPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["AmPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryAmPeakPercentObserved = row["AmPeakPercentObserved"] != null ? double.Parse(row["AmPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekendAmPeakAverageSpeed = row["WeekendAmPeakAverageSpeed"] != null ? double.Parse(row["WeekendAmPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakAverageEightyFifthSpeed = row["WeekendAmPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekendAmPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakViolations = row["WeekendAmPeakViolations"] != null ? int.Parse(row["WeekendAmPeakViolations"].ToString()) : (int?)null;
            var bigQueryWeekendAmPeakExtremeViolations = row["WeekendAmPeakExtremeViolations"] != null ? int.Parse(row["WeekendAmPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekendAmPeakFlow = row["WeekendAmPeakFlow"] != null ? int.Parse(row["WeekendAmPeakFlow"].ToString()) : (int?)null;
            var bigQueryWeekendAmPeakMinSpeed = row["WeekendAmPeakMinSpeed"] != null ? double.Parse(row["WeekendAmPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakMaxSpeed = row["WeekendAmPeakMaxSpeed"] != null ? double.Parse(row["WeekendAmPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakVariability = row["WeekendAmPeakVariability"] != null ? double.Parse(row["WeekendAmPeakVariability"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakPercentViolations = row["WeekendAmPeakPercentViolations"] != null ? double.Parse(row["WeekendAmPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakPercentExtremeViolations = row["WeekendAmPeakPercentExtremeViolations"] != null ? double.Parse(row["WeekendAmPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakAvgSpeedVsSpeedLimit = row["WeekendAmPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendAmPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakEightyFifthSpeedVsSpeedLimit = row["WeekendAmPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendAmPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendAmPeakPercentObserved = row["WeekendAmPeakPercentObserved"] != null ? double.Parse(row["WeekendAmPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekdayAmPeakAverageSpeed = row["WeekdayAmPeakAverageSpeed"] != null ? double.Parse(row["WeekdayAmPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakAverageEightyFifthSpeed = row["WeekdayAmPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekdayAmPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakViolations = row["WeekdayAmPeakViolations"] != null ? int.Parse(row["WeekdayAmPeakViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayAmPeakExtremeViolations = row["WeekdayAmPeakExtremeViolations"] != null ? int.Parse(row["WeekdayAmPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayAmPeakFlow = row["WeekdayAmPeakFlow"] != null ? int.Parse(row["WeekdayAmPeakFlow"].ToString()) : (int?)null;
            var bigQueryWeekdayAmPeakMinSpeed = row["WeekdayAmPeakMinSpeed"] != null ? double.Parse(row["WeekdayAmPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakMaxSpeed = row["WeekdayAmPeakMaxSpeed"] != null ? double.Parse(row["WeekdayAmPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakVariability = row["WeekdayAmPeakVariability"] != null ? double.Parse(row["WeekdayAmPeakVariability"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakPercentViolations = row["WeekdayAmPeakPercentViolations"] != null ? double.Parse(row["WeekdayAmPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakPercentExtremeViolations = row["WeekdayAmPeakPercentExtremeViolations"] != null ? double.Parse(row["WeekdayAmPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakAvgSpeedVsSpeedLimit = row["WeekdayAmPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayAmPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakEightyFifthSpeedVsSpeedLimit = row["WeekdayAmPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayAmPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayAmPeakPercentObserved = row["WeekdayAmPeakPercentObserved"] != null ? double.Parse(row["WeekdayAmPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryPmPeakAverageSpeed = row["PmPeakAverageSpeed"] != null ? double.Parse(row["PmPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryPmPeakAverageEightyFifthSpeed = row["PmPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["PmPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryPmPeakViolations = row["PmPeakViolations"] != null ? int.Parse(row["PmPeakViolations"].ToString()) : (int?)null;
            var bigQueryPmPeakExtremeViolations = row["PmPeakExtremeViolations"] != null ? int.Parse(row["PmPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryPmPeakFlow = row["PmPeakFlow"] != null ? int.Parse(row["PmPeakFlow"].ToString()) : (int?)null;
            var bigQueryPmPeakMinSpeed = row["PmPeakMinSpeed"] != null ? double.Parse(row["PmPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryPmPeakMaxSpeed = row["PmPeakMaxSpeed"] != null ? double.Parse(row["PmPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryPmPeakVariability = row["PmPeakVariability"] != null ? double.Parse(row["PmPeakVariability"].ToString()) : (double?)null;
            var bigQueryPmPeakPercentViolations = row["PmPeakPercentViolations"] != null ? double.Parse(row["PmPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryPmPeakPercentExtremeViolations = row["PmPeakPercentExtremeViolations"] != null ? double.Parse(row["PmPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryPmPeakAvgSpeedVsSpeedLimit = row["PmPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["PmPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryPmPeakEightyFifthSpeedVsSpeedLimit = row["PmPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["PmPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryPmPeakPercentObserved = row["PmPeakPercentObserved"] != null ? double.Parse(row["PmPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekendPmPeakAverageSpeed = row["WeekendPmPeakAverageSpeed"] != null ? double.Parse(row["WeekendPmPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakAverageEightyFifthSpeed = row["WeekendPmPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekendPmPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakViolations = row["WeekendPmPeakViolations"] != null ? int.Parse(row["WeekendPmPeakViolations"].ToString()) : (int?)null;
            var bigQueryWeekendPmPeakExtremeViolations = row["WeekendPmPeakExtremeViolations"] != null ? int.Parse(row["WeekendPmPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekendPmPeakFlow = row["WeekendPmPeakFlow"] != null ? int.Parse(row["WeekendPmPeakFlow"].ToString()) : (int?)null;
            var bigQueryWeekendPmPeakMinSpeed = row["WeekendPmPeakMinSpeed"] != null ? double.Parse(row["WeekendPmPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakMaxSpeed = row["WeekendPmPeakMaxSpeed"] != null ? double.Parse(row["WeekendPmPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakVariability = row["WeekendPmPeakVariability"] != null ? double.Parse(row["WeekendPmPeakVariability"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakPercentViolations = row["WeekendPmPeakPercentViolations"] != null ? double.Parse(row["WeekendPmPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakPercentExtremeViolations = row["WeekendPmPeakPercentExtremeViolations"] != null ? double.Parse(row["WeekendPmPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakAvgSpeedVsSpeedLimit = row["WeekendPmPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendPmPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakEightyFifthSpeedVsSpeedLimit = row["WeekendPmPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendPmPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendPmPeakPercentObserved = row["WeekendPmPeakPercentObserved"] != null ? double.Parse(row["WeekendPmPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekdayPmPeakAverageSpeed = row["WeekdayPmPeakAverageSpeed"] != null ? double.Parse(row["WeekdayPmPeakAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakAverageEightyFifthSpeed = row["WeekdayPmPeakAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekdayPmPeakAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakViolations = row["WeekdayPmPeakViolations"] != null ? int.Parse(row["WeekdayPmPeakViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayPmPeakExtremeViolations = row["WeekdayPmPeakExtremeViolations"] != null ? int.Parse(row["WeekdayPmPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayPmPeakFlow = row["WeekdayPmPeakFlow"] != null ? int.Parse(row["WeekdayPmPeakFlow"].ToString()) : (int?)null;
            var bigQueryWeekdayPmPeakMinSpeed = row["WeekdayPmPeakMinSpeed"] != null ? double.Parse(row["WeekdayPmPeakMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakMaxSpeed = row["WeekdayPmPeakMaxSpeed"] != null ? double.Parse(row["WeekdayPmPeakMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakVariability = row["WeekdayPmPeakVariability"] != null ? double.Parse(row["WeekdayPmPeakVariability"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakPercentViolations = row["WeekdayPmPeakPercentViolations"] != null ? double.Parse(row["WeekdayPmPeakPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakPercentExtremeViolations = row["WeekdayPmPeakPercentExtremeViolations"] != null ? double.Parse(row["WeekdayPmPeakPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakAvgSpeedVsSpeedLimit = row["WeekdayPmPeakAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayPmPeakAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakEightyFifthSpeedVsSpeedLimit = row["WeekdayPmPeakEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayPmPeakEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayPmPeakPercentObserved = row["WeekdayPmPeakPercentObserved"] != null ? double.Parse(row["WeekdayPmPeakPercentObserved"].ToString()) : (double?)null;

            var bigQueryMidDayAverageSpeed = row["MidDayAverageSpeed"] != null ? double.Parse(row["MidDayAverageSpeed"].ToString()) : (double?)null;
            var bigQueryMidDayAverageEightyFifthSpeed = row["MidDayAverageEightyFifthSpeed"] != null ? double.Parse(row["MidDayAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryMidDayViolations = row["MidDayViolations"] != null ? int.Parse(row["MidDayViolations"].ToString()) : (int?)null;
            var bigQueryMidDayExtremeViolations = row["MidDayExtremeViolations"] != null ? int.Parse(row["MidDayExtremeViolations"].ToString()) : (int?)null;
            var bigQueryMidDayFlow = row["MidDayFlow"] != null ? int.Parse(row["MidDayFlow"].ToString()) : (int?)null;
            var bigQueryMidDayMinSpeed = row["MidDayMinSpeed"] != null ? double.Parse(row["MidDayMinSpeed"].ToString()) : (double?)null;
            var bigQueryMidDayMaxSpeed = row["MidDayMaxSpeed"] != null ? double.Parse(row["MidDayMaxSpeed"].ToString()) : (double?)null;
            var bigQueryMidDayVariability = row["MidDayVariability"] != null ? double.Parse(row["MidDayVariability"].ToString()) : (double?)null;
            var bigQueryMidDayPercentViolations = row["MidDayPercentViolations"] != null ? double.Parse(row["MidDayPercentViolations"].ToString()) : (double?)null;
            var bigQueryMidDayPercentExtremeViolations = row["MidDayPercentExtremeViolations"] != null ? double.Parse(row["MidDayPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryMidDayAvgSpeedVsSpeedLimit = row["MidDayAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["MidDayAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryMidDayEightyFifthSpeedVsSpeedLimit = row["MidDayEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["MidDayEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryMidDayPercentObserved = row["MidDayEightyPercentObserved"] != null ? double.Parse(row["MidDayPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekendMidDayAverageSpeed = row["WeekendMidDayAverageSpeed"] != null ? double.Parse(row["WeekendMidDayAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayAverageEightyFifthSpeed = row["WeekendMidDayAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekendMidDayAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayViolations = row["WeekendMidDayViolations"] != null ? int.Parse(row["WeekendMidDayViolations"].ToString()) : (int?)null;
            var bigQueryWeekendMidDayExtremeViolations = row["WeekendMidDayExtremeViolations"] != null ? int.Parse(row["WeekendMidDayExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekendMidDayFlow = row["WeekendMidDayFlow"] != null ? int.Parse(row["WeekendMidDayFlow"].ToString()) : (int?)null;
            var bigQueryWeekendMidDayMinSpeed = row["WeekendMidDayMinSpeed"] != null ? double.Parse(row["WeekendMidDayMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayMaxSpeed = row["WeekendMidDayMaxSpeed"] != null ? double.Parse(row["WeekendMidDayMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayVariability = row["WeekendMidDayVariability"] != null ? double.Parse(row["WeekendMidDayVariability"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayPercentViolations = row["WeekendMidDayPercentViolations"] != null ? double.Parse(row["WeekendMidDayPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayPercentExtremeViolations = row["WeekendMidDayPercentExtremeViolations"] != null ? double.Parse(row["WeekendMidDayPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayAvgSpeedVsSpeedLimit = row["WeekendMidDayAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendMidDayAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayEightyFifthSpeedVsSpeedLimit = row["WeekendMidDayEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendMidDayEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendMidDayPercentObserved = row["WeekendMidDayPercentObserved"] != null ? double.Parse(row["WeekendMidDayPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekdayMidDayAverageSpeed = row["WeekdayMidDayAverageSpeed"] != null ? double.Parse(row["WeekdayMidDayAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayAverageEightyFifthSpeed = row["WeekdayMidDayAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekdayMidDayAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayViolations = row["WeekdayMidDayViolations"] != null ? int.Parse(row["WeekdayMidDayViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayMidDayExtremeViolations = row["WeekdayMidDayExtremeViolations"] != null ? int.Parse(row["WeekdayMidDayExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayMidDayFlow = row["WeekdayMidDayFlow"] != null ? int.Parse(row["WeekdayMidDayFlow"].ToString()) : (int?)null;
            var bigQueryWeekdayMidDayMinSpeed = row["WeekdayMidDayMinSpeed"] != null ? double.Parse(row["WeekdayMidDayMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayMaxSpeed = row["WeekdayMidDayMaxSpeed"] != null ? double.Parse(row["WeekdayMidDayMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayVariability = row["WeekdayMidDayVariability"] != null ? double.Parse(row["WeekdayMidDayVariability"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayPercentViolations = row["WeekdayMidDayPercentViolations"] != null ? double.Parse(row["WeekdayMidDayPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayPercentExtremeViolations = row["WeekdayMidDayPercentExtremeViolations"] != null ? double.Parse(row["WeekdayMidDayPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayAvgSpeedVsSpeedLimit = row["WeekdayMidDayAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayMidDayAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayEightyFifthSpeedVsSpeedLimit = row["WeekdayMidDayEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayMidDayEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayMidDayPercentObserved = row["WeekdayMidDayPercentObserved"] != null ? double.Parse(row["WeekdayMidDayPercentObserved"].ToString()) : (double?)null;

            var bigQueryEveningAverageSpeed = row["EveningAverageSpeed"] != null ? double.Parse(row["EveningAverageSpeed"].ToString()) : (double?)null;
            var bigQueryEveningAverageEightyFifthSpeed = row["EveningAverageEightyFifthSpeed"] != null ? double.Parse(row["EveningAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryEveningViolations = row["EveningViolations"] != null ? int.Parse(row["EveningViolations"].ToString()) : (int?)null;
            var bigQueryEveningExtremeViolations = row["EveningExtremeViolations"] != null ? int.Parse(row["EveningExtremeViolations"].ToString()) : (int?)null;
            var bigQueryEveningFlow = row["EveningFlow"] != null ? int.Parse(row["EveningFlow"].ToString()) : (int?)null;
            var bigQueryEveningMinSpeed = row["EveningMinSpeed"] != null ? double.Parse(row["EveningMinSpeed"].ToString()) : (double?)null;
            var bigQueryEveningMaxSpeed = row["EveningMaxSpeed"] != null ? double.Parse(row["EveningMaxSpeed"].ToString()) : (double?)null;
            var bigQueryEveningVariability = row["EveningVariability"] != null ? double.Parse(row["EveningVariability"].ToString()) : (double?)null;
            var bigQueryEveningPercentViolations = row["EveningPercentViolations"] != null ? double.Parse(row["EveningPercentViolations"].ToString()) : (double?)null;
            var bigQueryEveningPercentExtremeViolations = row["EveningPercentExtremeViolations"] != null ? double.Parse(row["EveningPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryEveningAvgSpeedVsSpeedLimit = row["EveningAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["EveningAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryEveningEightyFifthSpeedVsSpeedLimit = row["EveningEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["EveningEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryEveningPercentObserved = row["EveningPercentObserved"] != null ? double.Parse(row["EveningPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekendEveningAverageSpeed = row["WeekendEveningAverageSpeed"] != null ? double.Parse(row["WeekendEveningAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendEveningAverageEightyFifthSpeed = row["WeekendEveningAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekendEveningAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendEveningViolations = row["WeekendEveningViolations"] != null ? int.Parse(row["WeekendEveningViolations"].ToString()) : (int?)null;
            var bigQueryWeekendEveningExtremeViolations = row["WeekendEveningExtremeViolations"] != null ? int.Parse(row["WeekendEveningExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekendEveningFlow = row["WeekendEveningFlow"] != null ? int.Parse(row["WeekendEveningFlow"].ToString()) : (int?)null;
            var bigQueryWeekendEveningMinSpeed = row["WeekendEveningMinSpeed"] != null ? double.Parse(row["WeekendEveningMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendEveningMaxSpeed = row["WeekendEveningMaxSpeed"] != null ? double.Parse(row["WeekendEveningMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendEveningVariability = row["WeekendEveningVariability"] != null ? double.Parse(row["WeekendEveningVariability"].ToString()) : (double?)null;
            var bigQueryWeekendEveningPercentViolations = row["WeekendEveningPercentViolations"] != null ? double.Parse(row["WeekendEveningPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekendEveningPercentExtremeViolations = row["WeekendEveningPercentExtremeViolations"] != null ? double.Parse(row["WeekendEveningPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekendEveningAvgSpeedVsSpeedLimit = row["WeekendEveningAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendEveningAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendEveningEightyFifthSpeedVsSpeedLimit = row["WeekendEveningEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendEveningEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendEveningPercentObserved = row["WeekendEveningPercentObserved"] != null ? double.Parse(row["WeekendEveningPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekdayEveningAverageSpeed = row["WeekdayEveningAverageSpeed"] != null ? double.Parse(row["WeekdayEveningAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningAverageEightyFifthSpeed = row["WeekdayEveningAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekdayEveningAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningViolations = row["WeekdayEveningViolations"] != null ? int.Parse(row["WeekdayEveningViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayEveningExtremeViolations = row["WeekdayEveningExtremeViolations"] != null ? int.Parse(row["WeekdayEveningExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayEveningFlow = row["WeekdayEveningFlow"] != null ? int.Parse(row["WeekdayEveningFlow"].ToString()) : (int?)null;
            var bigQueryWeekdayEveningMinSpeed = row["WeekdayEveningMinSpeed"] != null ? double.Parse(row["WeekdayEveningMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningMaxSpeed = row["WeekdayEveningMaxSpeed"] != null ? double.Parse(row["WeekdayEveningMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningVariability = row["WeekdayEveningVariability"] != null ? double.Parse(row["WeekdayEveningVariability"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningPercentViolations = row["WeekdayEveningPercentViolations"] != null ? double.Parse(row["WeekdayEveningPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningPercentExtremeViolations = row["WeekdayEveningPercentExtremeViolations"] != null ? double.Parse(row["WeekdayEveningPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningAvgSpeedVsSpeedLimit = row["WeekdayEveningAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayEveningAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningEightyFifthSpeedVsSpeedLimit = row["WeekdayEveningEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayEveningEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayEveningPercentObserved = row["WeekdayEveningPercentObserved"] != null ? double.Parse(row["WeekdayEveningPercentObserved"].ToString()) : (double?)null;

            var bigQueryEarlyMorningAverageSpeed = row["EarlyMorningAverageSpeed"] != null ? double.Parse(row["EarlyMorningAverageSpeed"].ToString()) : (double?)null;
            var bigQueryEarlyMorningAverageEightyFifthSpeed = row["EarlyMorningAverageEightyFifthSpeed"] != null ? double.Parse(row["EarlyMorningAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryEarlyMorningViolations = row["EarlyMorningViolations"] != null ? int.Parse(row["EarlyMorningViolations"].ToString()) : (int?)null;
            var bigQueryEarlyMorningExtremeViolations = row["EarlyMorningExtremeViolations"] != null ? int.Parse(row["EarlyMorningExtremeViolations"].ToString()) : (int?)null;
            var bigQueryEarlyMorningFlow = row["EarlyMorningFlow"] != null ? int.Parse(row["EarlyMorningFlow"].ToString()) : (int?)null;
            var bigQueryEarlyMorningMinSpeed = row["EarlyMorningMinSpeed"] != null ? double.Parse(row["EarlyMorningMinSpeed"].ToString()) : (double?)null;
            var bigQueryEarlyMorningMaxSpeed = row["EarlyMorningMaxSpeed"] != null ? double.Parse(row["EarlyMorningMaxSpeed"].ToString()) : (double?)null;
            var bigQueryEarlyMorningVariability = row["EarlyMorningVariability"] != null ? double.Parse(row["EarlyMorningVariability"].ToString()) : (double?)null;
            var bigQueryEarlyMorningPercentViolations = row["EarlyMorningPercentViolations"] != null ? double.Parse(row["EarlyMorningPercentViolations"].ToString()) : (double?)null;
            var bigQueryEarlyMorningPercentExtremeViolations = row["EarlyMorningPercentExtremeViolations"] != null ? double.Parse(row["EarlyMorningPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryEarlyMorningAvgSpeedVsSpeedLimit = row["EarlyMorningAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["EarlyMorningAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryEarlyMorningEightyFifthSpeedVsSpeedLimit = row["EarlyMorningEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["EarlyMorningEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryEarlyMorningPercentObserved = row["EarlyMorningPercentObserved"] != null ? double.Parse(row["EarlyMorningPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekendEarlyMorningAverageSpeed = row["WeekendEarlyMorningAverageSpeed"] != null ? double.Parse(row["WeekendEarlyMorningAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningAverageEightyFifthSpeed = row["WeekendEarlyMorningAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekendEarlyMorningAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningViolations = row["WeekendEarlyMorningViolations"] != null ? int.Parse(row["WeekendEarlyMorningViolations"].ToString()) : (int?)null;
            var bigQueryWeekendEarlyMorningExtremeViolations = row["WeekendEarlyMorningExtremeViolations"] != null ? int.Parse(row["WeekendEarlyMorningExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekendEarlyMorningFlow = row["WeekendEarlyMorningFlow"] != null ? int.Parse(row["WeekendEarlyMorningFlow"].ToString()) : (int?)null;
            var bigQueryWeekendEarlyMorningMinSpeed = row["WeekendEarlyMorningMinSpeed"] != null ? double.Parse(row["WeekendEarlyMorningMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningMaxSpeed = row["WeekendEarlyMorningMaxSpeed"] != null ? double.Parse(row["WeekendEarlyMorningMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningVariability = row["WeekendEarlyMorningVariability"] != null ? double.Parse(row["WeekendEarlyMorningVariability"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningPercentViolations = row["WeekendEarlyMorningPercentViolations"] != null ? double.Parse(row["WeekendEarlyMorningPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningPercentExtremeViolations = row["WeekendEarlyMorningPercentExtremeViolations"] != null ? double.Parse(row["WeekendEarlyMorningPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningAvgSpeedVsSpeedLimit = row["WeekendEarlyMorningAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendEarlyMorningAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningEightyFifthSpeedVsSpeedLimit = row["WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekendEarlyMorningPercentObserved = row["WeekendEarlyMorningPercentObserved"] != null ? double.Parse(row["WeekendEarlyMorningPercentObserved"].ToString()) : (double?)null;

            var bigQueryWeekdayEarlyMorningAverageSpeed = row["WeekdayEarlyMorningAverageSpeed"] != null ? double.Parse(row["WeekdayEarlyMorningAverageSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningAverageEightyFifthSpeed = row["WeekdayEarlyMorningAverageEightyFifthSpeed"] != null ? double.Parse(row["WeekdayEarlyMorningAverageEightyFifthSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningViolations = row["WeekdayEarlyMorningViolations"] != null ? int.Parse(row["WeekdayEarlyMorningViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayEarlyMorningExtremeViolations = row["WeekdayEarlyMorningExtremeViolations"] != null ? int.Parse(row["WeekdayEarlyMorningExtremeViolations"].ToString()) : (int?)null;
            var bigQueryWeekdayEarlyMorningFlow = row["WeekdayEarlyMorningFlow"] != null ? int.Parse(row["WeekdayEarlyMorningFlow"].ToString()) : (int?)null;
            var bigQueryWeekdayEarlyMorningMinSpeed = row["WeekdayEarlyMorningMinSpeed"] != null ? double.Parse(row["WeekdayEarlyMorningMinSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningMaxSpeed = row["WeekdayEarlyMorningMaxSpeed"] != null ? double.Parse(row["WeekdayEarlyMorningMaxSpeed"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningVariability = row["WeekdayEarlyMorningVariability"] != null ? double.Parse(row["WeekdayEarlyMorningVariability"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningPercentViolations = row["WeekdayEarlyMorningPercentViolations"] != null ? double.Parse(row["WeekdayEarlyMorningPercentViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningPercentExtremeViolations = row["WeekdayEarlyMorningPercentExtremeViolations"] != null ? double.Parse(row["WeekdayEarlyMorningPercentExtremeViolations"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningAvgSpeedVsSpeedLimit = row["WeekdayEarlyMorningAvgSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayEarlyMorningAvgSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit = row["WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit"] != null ? double.Parse(row["WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit"].ToString()) : (double?)null;
            var bigQueryWeekdayEarlyMorningPercentObserved = row["WeekdayEarlyMorningPercentObserved"] != null ? double.Parse(row["WeekdayEarlyMorningPercentObserved"].ToString()) : (double?)null;

            var bigQueryPercentObserved = row["PercentObserved"] != null ? double.Parse(row["PercentObserved"].ToString()) : (double?)null;

            return new MonthlyAggregation
            {
                Id = bigQueryId,
                CreatedDate = bigQueryCreatedDate,
                BinStartTime = bigQueryBinStartTime,
                SegmentId = bigQuerySegmentId,
                SourceId = bigQuerySourceId,

                AllDayAverageSpeed = bigQueryAllDayAverageSpeed,
                AllDayAverageEightyFifthSpeed = bigQueryAllDayAverageEightyFifthSpeed,
                AllDayViolations = bigQueryAllDayViolations,
                AllDayExtremeViolations = bigQueryAllDayExtremeViolations,
                AllDayFlow = bigQueryAllDayFlow,
                AllDayMinSpeed = bigQueryAllDayMinSpeed,
                AllDayMaxSpeed = bigQueryAllDayMaxSpeed,
                AllDayVariability = bigQueryAllDayVariability,
                AllDayPercentViolations = bigQueryAllDayPercentViolations,
                AllDayPercentExtremeViolations = bigQueryAllDayPercentExtremeViolations,
                AllDayAvgSpeedVsSpeedLimit = bigQueryAllDayAvgSpeedVsSpeedLimit,
                AllDayEightyFifthSpeedVsSpeedLimit = bigQueryAllDayEightyFifthSpeedVsSpeedLimit,
                AllDayPercentObserved = bigQueryAllDayPercentObserved,

                WeekendAllDayAverageSpeed = bigQueryWeekendAllDayAverageSpeed,
                WeekendAllDayAverageEightyFifthSpeed = bigQueryWeekendAllDayAverageEightyFifthSpeed,
                WeekendAllDayViolations = bigQueryWeekendAllDayViolations,
                WeekendAllDayExtremeViolations = bigQueryWeekendAllDayExtremeViolations,
                WeekendAllDayFlow = bigQueryWeekendAllDayFlow,
                WeekendAllDayMinSpeed = bigQueryWeekendAllDayMinSpeed,
                WeekendAllDayMaxSpeed = bigQueryWeekendAllDayMaxSpeed,
                WeekendAllDayVariability = bigQueryWeekendAllDayVariability,
                WeekendAllDayPercentViolations = bigQueryWeekendAllDayPercentViolations,
                WeekendAllDayPercentExtremeViolations = bigQueryWeekendAllDayPercentExtremeViolations,
                WeekendAllDayAvgSpeedVsSpeedLimit = bigQueryWeekendAllDayAvgSpeedVsSpeedLimit,
                WeekendAllDayEightyFifthSpeedVsSpeedLimit = bigQueryWeekendAllDayEightyFifthSpeedVsSpeedLimit,
                WeekendAllDayPercentObserved = bigQueryWeekendAllDayPercentObserved,

                WeekdayAllDayAverageSpeed = bigQueryWeekdayAllDayAverageSpeed,
                WeekdayAllDayAverageEightyFifthSpeed = bigQueryWeekdayAllDayAverageEightyFifthSpeed,
                WeekdayAllDayViolations = bigQueryWeekdayAllDayViolations,
                WeekdayAllDayExtremeViolations = bigQueryWeekdayAllDayExtremeViolations,
                WeekdayAllDayFlow = bigQueryWeekdayAllDayFlow,
                WeekdayAllDayMinSpeed = bigQueryWeekdayAllDayMinSpeed,
                WeekdayAllDayMaxSpeed = bigQueryWeekdayAllDayMaxSpeed,
                WeekdayAllDayVariability = bigQueryWeekdayAllDayVariability,
                WeekdayAllDayPercentViolations = bigQueryWeekdayAllDayPercentViolations,
                WeekdayAllDayPercentExtremeViolations = bigQueryWeekdayAllDayPercentExtremeViolations,
                WeekdayAllDayAvgSpeedVsSpeedLimit = bigQueryWeekdayAllDayAvgSpeedVsSpeedLimit,
                WeekdayAllDayEightyFifthSpeedVsSpeedLimit = bigQueryWeekdayAllDayEightyFifthSpeedVsSpeedLimit,
                WeekdayAllDayPercentObserved = bigQueryWeekdayAllDayPercentObserved,

                OffPeakAverageSpeed = bigQueryOffPeakAverageSpeed,
                OffPeakAverageEightyFifthSpeed = bigQueryOffPeakAverageEightyFifthSpeed,
                OffPeakViolations = bigQueryOffPeakViolations,
                OffPeakExtremeViolations = bigQueryOffPeakExtremeViolations,
                OffPeakFlow = bigQueryOffPeakFlow,
                OffPeakMinSpeed = bigQueryOffPeakMinSpeed,
                OffPeakMaxSpeed = bigQueryOffPeakMaxSpeed,
                OffPeakVariability = bigQueryOffPeakVariability,
                OffPeakPercentViolations = bigQueryOffPeakPercentViolations,
                OffPeakPercentExtremeViolations = bigQueryOffPeakPercentExtremeViolations,
                OffPeakAvgSpeedVsSpeedLimit = bigQueryOffPeakAvgSpeedVsSpeedLimit,
                OffPeakEightyFifthSpeedVsSpeedLimit = bigQueryOffPeakEightyFifthSpeedVsSpeedLimit,
                OffPeakPercentObserved = bigQueryOffPeakPercentObserved,

                WeekendOffPeakAverageSpeed = bigQueryWeekendOffPeakAverageSpeed,
                WeekendOffPeakAverageEightyFifthSpeed = bigQueryWeekendOffPeakAverageEightyFifthSpeed,
                WeekendOffPeakViolations = bigQueryWeekendOffPeakViolations,
                WeekendOffPeakExtremeViolations = bigQueryWeekendOffPeakExtremeViolations,
                WeekendOffPeakFlow = bigQueryWeekendOffPeakFlow,
                WeekendOffPeakMinSpeed = bigQueryWeekendOffPeakMinSpeed,
                WeekendOffPeakMaxSpeed = bigQueryWeekendOffPeakMaxSpeed,
                WeekendOffPeakVariability = bigQueryWeekendOffPeakVariability,
                WeekendOffPeakPercentViolations = bigQueryWeekendOffPeakPercentViolations,
                WeekendOffPeakPercentExtremeViolations = bigQueryWeekendOffPeakPercentExtremeViolations,
                WeekendOffPeakAvgSpeedVsSpeedLimit = bigQueryWeekendOffPeakAvgSpeedVsSpeedLimit,
                WeekendOffPeakEightyFifthSpeedVsSpeedLimit = bigQueryWeekendOffPeakEightyFifthSpeedVsSpeedLimit,
                WeekendOffPeakPercentObserved = bigQueryWeekendOffPeakPercentObserved,

                WeekdayOffPeakAverageSpeed = bigQueryWeekdayOffPeakAverageSpeed,
                WeekdayOffPeakAverageEightyFifthSpeed = bigQueryWeekdayOffPeakAverageEightyFifthSpeed,
                WeekdayOffPeakViolations = bigQueryWeekdayOffPeakViolations,
                WeekdayOffPeakExtremeViolations = bigQueryWeekdayOffPeakExtremeViolations,
                WeekdayOffPeakFlow = bigQueryWeekdayOffPeakFlow,
                WeekdayOffPeakMinSpeed = bigQueryWeekdayOffPeakMinSpeed,
                WeekdayOffPeakMaxSpeed = bigQueryWeekdayOffPeakMaxSpeed,
                WeekdayOffPeakVariability = bigQueryWeekdayOffPeakVariability,
                WeekdayOffPeakPercentViolations = bigQueryWeekdayOffPeakPercentViolations,
                WeekdayOffPeakPercentExtremeViolations = bigQueryWeekdayOffPeakPercentExtremeViolations,
                WeekdayOffPeakAvgSpeedVsSpeedLimit = bigQueryWeekdayOffPeakAvgSpeedVsSpeedLimit,
                WeekdayOffPeakEightyFifthSpeedVsSpeedLimit = bigQueryWeekdayOffPeakEightyFifthSpeedVsSpeedLimit,
                WeekdayOffPeakPercentObserved = bigQueryWeekdayOffPeakPercentObserved,

                AmPeakAverageSpeed = bigQueryAmPeakAverageSpeed,
                AmPeakAverageEightyFifthSpeed = bigQueryAmPeakAverageEightyFifthSpeed,
                AmPeakViolations = bigQueryAmPeakViolations,
                AmPeakExtremeViolations = bigQueryAmPeakExtremeViolations,
                AmPeakFlow = bigQueryAmPeakFlow,
                AmPeakMinSpeed = bigQueryAmPeakMinSpeed,
                AmPeakMaxSpeed = bigQueryAmPeakMaxSpeed,
                AmPeakVariability = bigQueryAmPeakVariability,
                AmPeakPercentViolations = bigQueryAmPeakPercentViolations,
                AmPeakPercentExtremeViolations = bigQueryAmPeakPercentExtremeViolations,
                AmPeakAvgSpeedVsSpeedLimit = bigQueryAmPeakAvgSpeedVsSpeedLimit,
                AmPeakEightyFifthSpeedVsSpeedLimit = bigQueryAmPeakEightyFifthSpeedVsSpeedLimit,
                AmPeakPercentObserved = bigQueryAmPeakPercentObserved,

                WeekendAmPeakAverageSpeed = bigQueryWeekendAmPeakAverageSpeed,
                WeekendAmPeakAverageEightyFifthSpeed = bigQueryWeekendAmPeakAverageEightyFifthSpeed,
                WeekendAmPeakViolations = bigQueryWeekendAmPeakViolations,
                WeekendAmPeakExtremeViolations = bigQueryWeekendAmPeakExtremeViolations,
                WeekendAmPeakFlow = bigQueryWeekendAmPeakFlow,
                WeekendAmPeakMinSpeed = bigQueryWeekendAmPeakMinSpeed,
                WeekendAmPeakMaxSpeed = bigQueryWeekendAmPeakMaxSpeed,
                WeekendAmPeakVariability = bigQueryWeekendAmPeakVariability,
                WeekendAmPeakPercentViolations = bigQueryWeekendAmPeakPercentViolations,
                WeekendAmPeakPercentExtremeViolations = bigQueryWeekendAmPeakPercentExtremeViolations,
                WeekendAmPeakAvgSpeedVsSpeedLimit = bigQueryWeekendAmPeakAvgSpeedVsSpeedLimit,
                WeekendAmPeakEightyFifthSpeedVsSpeedLimit = bigQueryWeekendAmPeakEightyFifthSpeedVsSpeedLimit,
                WeekendAmPeakPercentObserved = bigQueryWeekendAmPeakPercentObserved,

                WeekdayAmPeakAverageSpeed = bigQueryWeekdayAmPeakAverageSpeed,
                WeekdayAmPeakAverageEightyFifthSpeed = bigQueryWeekdayAmPeakAverageEightyFifthSpeed,
                WeekdayAmPeakViolations = bigQueryWeekdayAmPeakViolations,
                WeekdayAmPeakExtremeViolations = bigQueryWeekdayAmPeakExtremeViolations,
                WeekdayAmPeakFlow = bigQueryWeekdayAmPeakFlow,
                WeekdayAmPeakMinSpeed = bigQueryWeekdayAmPeakMinSpeed,
                WeekdayAmPeakMaxSpeed = bigQueryWeekdayAmPeakMaxSpeed,
                WeekdayAmPeakVariability = bigQueryWeekdayAmPeakVariability,
                WeekdayAmPeakPercentViolations = bigQueryWeekdayAmPeakPercentViolations,
                WeekdayAmPeakPercentExtremeViolations = bigQueryWeekdayAmPeakPercentExtremeViolations,
                WeekdayAmPeakAvgSpeedVsSpeedLimit = bigQueryWeekdayAmPeakAvgSpeedVsSpeedLimit,
                WeekdayAmPeakEightyFifthSpeedVsSpeedLimit = bigQueryWeekdayAmPeakEightyFifthSpeedVsSpeedLimit,
                WeekdayAmPeakPercentObserved = bigQueryWeekdayAmPeakPercentObserved,

                PmPeakAverageSpeed = bigQueryPmPeakAverageSpeed,
                PmPeakAverageEightyFifthSpeed = bigQueryPmPeakAverageEightyFifthSpeed,
                PmPeakViolations = bigQueryPmPeakViolations,
                PmPeakExtremeViolations = bigQueryPmPeakExtremeViolations,
                PmPeakFlow = bigQueryPmPeakFlow,
                PmPeakMinSpeed = bigQueryPmPeakMinSpeed,
                PmPeakMaxSpeed = bigQueryPmPeakMaxSpeed,
                PmPeakVariability = bigQueryPmPeakVariability,
                PmPeakPercentViolations = bigQueryPmPeakPercentViolations,
                PmPeakPercentExtremeViolations = bigQueryPmPeakPercentExtremeViolations,
                PmPeakAvgSpeedVsSpeedLimit = bigQueryPmPeakAvgSpeedVsSpeedLimit,
                PmPeakEightyFifthSpeedVsSpeedLimit = bigQueryPmPeakEightyFifthSpeedVsSpeedLimit,
                PmPeakPercentObserved = bigQueryPmPeakPercentObserved,

                WeekendPmPeakAverageSpeed = bigQueryWeekendPmPeakAverageSpeed,
                WeekendPmPeakAverageEightyFifthSpeed = bigQueryWeekendPmPeakAverageEightyFifthSpeed,
                WeekendPmPeakViolations = bigQueryWeekendPmPeakViolations,
                WeekendPmPeakExtremeViolations = bigQueryWeekendPmPeakExtremeViolations,
                WeekendPmPeakFlow = bigQueryWeekendPmPeakFlow,
                WeekendPmPeakMinSpeed = bigQueryWeekendPmPeakMinSpeed,
                WeekendPmPeakMaxSpeed = bigQueryWeekendPmPeakMaxSpeed,
                WeekendPmPeakVariability = bigQueryWeekendPmPeakVariability,
                WeekendPmPeakPercentViolations = bigQueryWeekendPmPeakPercentViolations,
                WeekendPmPeakPercentExtremeViolations = bigQueryWeekendPmPeakPercentExtremeViolations,
                WeekendPmPeakAvgSpeedVsSpeedLimit = bigQueryWeekendPmPeakAvgSpeedVsSpeedLimit,
                WeekendPmPeakEightyFifthSpeedVsSpeedLimit = bigQueryWeekendPmPeakEightyFifthSpeedVsSpeedLimit,
                WeekendPmPeakPercentObserved = bigQueryWeekendPmPeakPercentObserved,

                WeekdayPmPeakAverageSpeed = bigQueryWeekdayPmPeakAverageSpeed,
                WeekdayPmPeakAverageEightyFifthSpeed = bigQueryWeekdayPmPeakAverageEightyFifthSpeed,
                WeekdayPmPeakViolations = bigQueryWeekdayPmPeakViolations,
                WeekdayPmPeakExtremeViolations = bigQueryWeekdayPmPeakExtremeViolations,
                WeekdayPmPeakFlow = bigQueryWeekdayPmPeakFlow,
                WeekdayPmPeakMinSpeed = bigQueryWeekdayPmPeakMinSpeed,
                WeekdayPmPeakMaxSpeed = bigQueryWeekdayPmPeakMaxSpeed,
                WeekdayPmPeakVariability = bigQueryWeekdayPmPeakVariability,
                WeekdayPmPeakPercentViolations = bigQueryWeekdayPmPeakPercentViolations,
                WeekdayPmPeakPercentExtremeViolations = bigQueryWeekdayPmPeakPercentExtremeViolations,
                WeekdayPmPeakAvgSpeedVsSpeedLimit = bigQueryWeekdayPmPeakAvgSpeedVsSpeedLimit,
                WeekdayPmPeakEightyFifthSpeedVsSpeedLimit = bigQueryWeekdayPmPeakEightyFifthSpeedVsSpeedLimit,
                WeekdayPmPeakPercentObserved = bigQueryWeekdayPmPeakPercentObserved,

                MidDayAverageSpeed = bigQueryMidDayAverageSpeed,
                MidDayAverageEightyFifthSpeed = bigQueryMidDayAverageEightyFifthSpeed,
                MidDayViolations = bigQueryMidDayViolations,
                MidDayExtremeViolations = bigQueryMidDayExtremeViolations,
                MidDayFlow = bigQueryMidDayFlow,
                MidDayMinSpeed = bigQueryMidDayMinSpeed,
                MidDayMaxSpeed = bigQueryMidDayMaxSpeed,
                MidDayVariability = bigQueryMidDayVariability,
                MidDayPercentViolations = bigQueryMidDayPercentViolations,
                MidDayPercentExtremeViolations = bigQueryMidDayPercentExtremeViolations,
                MidDayAvgSpeedVsSpeedLimit = bigQueryMidDayAvgSpeedVsSpeedLimit,
                MidDayEightyFifthSpeedVsSpeedLimit = bigQueryMidDayEightyFifthSpeedVsSpeedLimit,
                MidDayPercentObserved = bigQueryMidDayPercentObserved,

                WeekendMidDayAverageSpeed = bigQueryWeekendMidDayAverageSpeed,
                WeekendMidDayAverageEightyFifthSpeed = bigQueryWeekendMidDayAverageEightyFifthSpeed,
                WeekendMidDayViolations = bigQueryWeekendMidDayViolations,
                WeekendMidDayExtremeViolations = bigQueryWeekendMidDayExtremeViolations,
                WeekendMidDayFlow = bigQueryWeekendMidDayFlow,
                WeekendMidDayMinSpeed = bigQueryWeekendMidDayMinSpeed,
                WeekendMidDayMaxSpeed = bigQueryWeekendMidDayMaxSpeed,
                WeekendMidDayVariability = bigQueryWeekendMidDayVariability,
                WeekendMidDayPercentViolations = bigQueryWeekendMidDayPercentViolations,
                WeekendMidDayPercentExtremeViolations = bigQueryWeekendMidDayPercentExtremeViolations,
                WeekendMidDayAvgSpeedVsSpeedLimit = bigQueryWeekendMidDayAvgSpeedVsSpeedLimit,
                WeekendMidDayEightyFifthSpeedVsSpeedLimit = bigQueryWeekendMidDayEightyFifthSpeedVsSpeedLimit,
                WeekendMidDayPercentObserved = bigQueryWeekendPercentObserved,

                WeekdayMidDayAverageSpeed = bigQueryWeekdayMidDayAverageSpeed,
                WeekdayMidDayAverageEightyFifthSpeed = bigQueryWeekdayMidDayAverageEightyFifthSpeed,
                WeekdayMidDayViolations = bigQueryWeekdayMidDayViolations,
                WeekdayMidDayExtremeViolations = bigQueryWeekdayMidDayExtremeViolations,
                WeekdayMidDayFlow = bigQueryWeekdayMidDayFlow,
                WeekdayMidDayMinSpeed = bigQueryWeekdayMidDayMinSpeed,
                WeekdayMidDayMaxSpeed = bigQueryWeekdayMidDayMaxSpeed,
                WeekdayMidDayVariability = bigQueryWeekdayMidDayVariability,
                WeekdayMidDayPercentViolations = bigQueryWeekdayMidDayPercentViolations,
                WeekdayMidDayPercentExtremeViolations = bigQueryWeekdayMidDayPercentExtremeViolations,
                WeekdayMidDayAvgSpeedVsSpeedLimit = bigQueryWeekdayMidDayAvgSpeedVsSpeedLimit,
                WeekdayMidDayEightyFifthSpeedVsSpeedLimit = bigQueryWeekdayMidDayEightyFifthSpeedVsSpeedLimit,
                WeekdayMidDayPercentObserved = bigQueryWeekdayPercentObserved,

                EveningAverageSpeed = bigQueryEveningAverageSpeed,
                EveningAverageEightyFifthSpeed = bigQueryEveningAverageEightyFifthSpeed,
                EveningViolations = bigQueryEveningViolations,
                EveningExtremeViolations = bigQueryEveningExtremeViolations,
                EveningFlow = bigQueryEveningFlow,
                EveningMinSpeed = bigQueryEveningMinSpeed,
                EveningMaxSpeed = bigQueryEveningMaxSpeed,
                EveningVariability = bigQueryEveningVariability,
                EveningPercentViolations = bigQueryEveningPercentViolations,
                EveningPercentExtremeViolations = bigQueryEveningPercentExtremeViolations,
                EveningAvgSpeedVsSpeedLimit = bigQueryEveningAvgSpeedVsSpeedLimit,
                EveningEightyFifthSpeedVsSpeedLimit = bigQueryEveningEightyFifthSpeedVsSpeedLimit,
                EveningPercentObserved = bigQueryEveningPercentObserved,

                WeekendEveningAverageSpeed = bigQueryWeekendEveningAverageSpeed,
                WeekendEveningAverageEightyFifthSpeed = bigQueryWeekendEveningAverageEightyFifthSpeed,
                WeekendEveningViolations = bigQueryWeekendEveningViolations,
                WeekendEveningExtremeViolations = bigQueryWeekendEveningExtremeViolations,
                WeekendEveningFlow = bigQueryWeekendEveningFlow,
                WeekendEveningMinSpeed = bigQueryWeekendEveningMinSpeed,
                WeekendEveningMaxSpeed = bigQueryWeekendEveningMaxSpeed,
                WeekendEveningVariability = bigQueryWeekendEveningVariability,
                WeekendEveningPercentViolations = bigQueryWeekendEveningPercentViolations,
                WeekendEveningPercentExtremeViolations = bigQueryWeekendEveningPercentExtremeViolations,
                WeekendEveningAvgSpeedVsSpeedLimit = bigQueryWeekendEveningAvgSpeedVsSpeedLimit,
                WeekendEveningEightyFifthSpeedVsSpeedLimit = bigQueryWeekendEveningEightyFifthSpeedVsSpeedLimit,
                WeekendEveningPercentObserved = bigQueryWeekendEveningPercentObserved,

                WeekdayEveningAverageSpeed = bigQueryWeekdayEveningAverageSpeed,
                WeekdayEveningAverageEightyFifthSpeed = bigQueryWeekdayEveningAverageEightyFifthSpeed,
                WeekdayEveningViolations = bigQueryWeekdayEveningViolations,
                WeekdayEveningExtremeViolations = bigQueryWeekdayEveningExtremeViolations,
                WeekdayEveningFlow = bigQueryWeekdayEveningFlow,
                WeekdayEveningMinSpeed = bigQueryWeekdayEveningMinSpeed,
                WeekdayEveningMaxSpeed = bigQueryWeekdayEveningMaxSpeed,
                WeekdayEveningVariability = bigQueryWeekdayEveningVariability,
                WeekdayEveningPercentViolations = bigQueryWeekdayEveningPercentViolations,
                WeekdayEveningPercentExtremeViolations = bigQueryWeekdayEveningPercentExtremeViolations,
                WeekdayEveningAvgSpeedVsSpeedLimit = bigQueryWeekdayEveningAvgSpeedVsSpeedLimit,
                WeekdayEveningEightyFifthSpeedVsSpeedLimit = bigQueryWeekdayEveningEightyFifthSpeedVsSpeedLimit,
                WeekdayEveningPercentObserved = bigQueryWeekdayEveningPercentObserved,

                EarlyMorningAverageSpeed = bigQueryEarlyMorningAverageSpeed,
                EarlyMorningAverageEightyFifthSpeed = bigQueryEarlyMorningAverageEightyFifthSpeed,
                EarlyMorningViolations = bigQueryEarlyMorningViolations,
                EarlyMorningExtremeViolations = bigQueryEarlyMorningExtremeViolations,
                EarlyMorningFlow = bigQueryEarlyMorningFlow,
                EarlyMorningMinSpeed = bigQueryEarlyMorningMinSpeed,
                EarlyMorningMaxSpeed = bigQueryEarlyMorningMaxSpeed,
                EarlyMorningVariability = bigQueryEarlyMorningVariability,
                EarlyMorningPercentViolations = bigQueryEarlyMorningPercentViolations,
                EarlyMorningPercentExtremeViolations = bigQueryEarlyMorningPercentExtremeViolations,
                EarlyMorningAvgSpeedVsSpeedLimit = bigQueryEarlyMorningAvgSpeedVsSpeedLimit,
                EarlyMorningEightyFifthSpeedVsSpeedLimit = bigQueryEarlyMorningEightyFifthSpeedVsSpeedLimit,
                EarlyMorningPercentObserved = bigQueryEarlyMorningPercentObserved,

                WeekendEarlyMorningAverageSpeed = bigQueryWeekendEarlyMorningAverageSpeed,
                WeekendEarlyMorningAverageEightyFifthSpeed = bigQueryWeekendEarlyMorningAverageEightyFifthSpeed,
                WeekendEarlyMorningViolations = bigQueryWeekendEarlyMorningViolations,
                WeekendEarlyMorningExtremeViolations = bigQueryWeekendEarlyMorningExtremeViolations,
                WeekendEarlyMorningFlow = bigQueryWeekendEarlyMorningFlow,
                WeekendEarlyMorningMinSpeed = bigQueryWeekendEarlyMorningMinSpeed,
                WeekendEarlyMorningMaxSpeed = bigQueryWeekendEarlyMorningMaxSpeed,
                WeekendEarlyMorningVariability = bigQueryWeekendEarlyMorningVariability,
                WeekendEarlyMorningPercentViolations = bigQueryWeekendEarlyMorningPercentViolations,
                WeekendEarlyMorningPercentExtremeViolations = bigQueryWeekendEarlyMorningPercentExtremeViolations,
                WeekendEarlyMorningAvgSpeedVsSpeedLimit = bigQueryWeekendEarlyMorningAvgSpeedVsSpeedLimit,
                WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit = bigQueryWeekendEarlyMorningEightyFifthSpeedVsSpeedLimit,
                WeekendEarlyMorningPercentObserved = bigQueryWeekendEarlyMorningPercentObserved,

                WeekdayEarlyMorningAverageSpeed = bigQueryWeekdayEarlyMorningAverageSpeed,
                WeekdayEarlyMorningAverageEightyFifthSpeed = bigQueryWeekdayEarlyMorningAverageEightyFifthSpeed,
                WeekdayEarlyMorningViolations = bigQueryWeekdayEarlyMorningViolations,
                WeekdayEarlyMorningExtremeViolations = bigQueryWeekdayEarlyMorningExtremeViolations,
                WeekdayEarlyMorningFlow = bigQueryWeekdayEarlyMorningFlow,
                WeekdayEarlyMorningMinSpeed = bigQueryWeekdayEarlyMorningMinSpeed,
                WeekdayEarlyMorningMaxSpeed = bigQueryWeekdayEarlyMorningMaxSpeed,
                WeekdayEarlyMorningVariability = bigQueryWeekdayEarlyMorningVariability,
                WeekdayEarlyMorningPercentViolations = bigQueryWeekdayEarlyMorningPercentViolations,
                WeekdayEarlyMorningPercentExtremeViolations = bigQueryWeekdayEarlyMorningPercentExtremeViolations,
                WeekdayEarlyMorningAvgSpeedVsSpeedLimit = bigQueryWeekdayEarlyMorningAvgSpeedVsSpeedLimit,
                WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit = bigQueryWeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit,
                WeekdayEarlyMorningPercentObserved = bigQueryWeekdayEarlyMorningPercentObserved
            };
        }


        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

        private string InsertQueryStatement(MonthlyAggregation item)
        {
            return $"INSERT INTO `{_datasetId}.{_tableId}` " +
                $"(Id, CreatedDate, BinStartTime, SegmentId, SourceId, " +
                $"AllDayAverageSpeed, AllDayAverageEightyFifthSpeed, AllDayViolations, AllDayExtremeViolations, AllDayFlow, AllDayMinSpeed, AllDayMaxSpeed, AllDayVariability, AllDayPercentViolations, AllDayPercentExtremeViolations, AllDayAvgSpeedVsSpeedLimit, AllDayEightyFifthSpeedVsSpeedLimit, AllDayPercentObserved, " +
                $"WeekendAllDayAverageSpeed, WeekendAllDayAverageEightyFifthSpeed, WeekendAllDayViolations, WeekendAllDayExtremeViolations, WeekendAllDayFlow, WeekendAllDayMinSpeed, WeekendAllDayMaxSpeed, WeekendAllDayVariability, WeekendAllDayPercentViolations, WeekendAllDayPercentExtremeViolations, WeekendAllDayAvgSpeedVsSpeedLimit, WeekendAllDayEightyFifthSpeedVsSpeedLimit, WeekendAllDayPercentObserved, " +
                $"WeekdayAllDayAverageSpeed, WeekdayAllDayAverageEightyFifthSpeed, WeekdayAllDayViolations, WeekdayAllDayExtremeViolations, WeekdayAllDayFlow, WeekdayAllDayMinSpeed, WeekdayAllDayMaxSpeed, WeekdayAllDayVariability, WeekdayAllDayPercentViolations, WeekdayAllDayPercentExtremeViolations, WeekdayAllDayAvgSpeedVsSpeedLimit, WeekdayAllDayEightyFifthSpeedVsSpeedLimit, WeekdayAllDayPercentObserved, " +

                $"OffPeakAverageSpeed, OffPeakAverageEightyFifthSpeed, OffPeakViolations, OffPeakExtremeViolations, OffPeakFlow, OffPeakMinSpeed, OffPeakMaxSpeed, OffPeakVariability, OffPeakPercentViolations, OffPeakPercentExtremeViolations, OffPeakAvgSpeedVsSpeedLimit, OffPeakEightyFifthSpeedVsSpeedLimit, OffPeakPercentObserved, " +
                $"WeekendOffPeakAverageSpeed, WeekendOffPeakAverageEightyFifthSpeed, WeekendOffPeakViolations, WeekendOffPeakExtremeViolations, WeekendOffPeakFlow, WeekendOffPeakMinSpeed, WeekendOffPeakMaxSpeed, WeekendOffPeakVariability, WeekendOffPeakPercentViolations, WeekendOffPeakPercentExtremeViolations, WeekendOffPeakAvgSpeedVsSpeedLimit, WeekendOffPeakEightyFifthSpeedVsSpeedLimit, WeekendOffPeakPercentObserved, " +
                $"WeekdayOffPeakAverageSpeed, WeekdayOffPeakAverageEightyFifthSpeed, WeekdayOffPeakViolations, WeekdayOffPeakExtremeViolations, WeekdayOffPeakFlow, WeekdayOffPeakMinSpeed, WeekdayOffPeakMaxSpeed, WeekdayOffPeakVariability, WeekdayOffPeakPercentViolations, WeekdayOffPeakPercentExtremeViolations, WeekdayOffPeakAvgSpeedVsSpeedLimit, WeekdayOffPeakEightyFifthSpeedVsSpeedLimit, WeekdayOffPeakPercentObserved, " +

                $"AmPeakAverageSpeed, AmPeakAverageEightyFifthSpeed, AmPeakViolations, AmPeakExtremeViolations, AmPeakFlow, AmPeakMinSpeed, AmPeakMaxSpeed, AmPeakVariability, AmPeakPercentViolations, AmPeakPercentExtremeViolations, AmPeakAvgSpeedVsSpeedLimit, AmPeakEightyFifthSpeedVsSpeedLimit, AmPeakPercentObserved, " +
                $"WeekendAmPeakAverageSpeed, WeekendAmPeakAverageEightyFifthSpeed, WeekendAmPeakViolations, WeekendAmPeakExtremeViolations, WeekendAmPeakFlow, WeekendAmPeakMinSpeed, WeekendAmPeakMaxSpeed, WeekendAmPeakVariability, WeekendAmPeakPercentViolations, WeekendAmPeakPercentExtremeViolations, WeekendAmPeakAvgSpeedVsSpeedLimit, WeekendAmPeakEightyFifthSpeedVsSpeedLimit, WeekendAmPeakPercentObserved, " +
                $"WeekdayAmPeakAverageSpeed, WeekdayAmPeakAverageEightyFifthSpeed, WeekdayAmPeakViolations, WeekdayAmPeakExtremeViolations, WeekdayAmPeakFlow, WeekdayAmPeakMinSpeed, WeekdayAmPeakMaxSpeed, WeekdayAmPeakVariability, WeekdayAmPeakPercentViolations, WeekdayAmPeakPercentExtremeViolations, WeekdayAmPeakAvgSpeedVsSpeedLimit, WeekdayAmPeakEightyFifthSpeedVsSpeedLimit, WeekdayAmPeakPercentObserved, " +

                $"PmPeakAverageSpeed, PmPeakAverageEightyFifthSpeed, PmPeakViolations, PmPeakExtremeViolations, PmPeakFlow, PmPeakMinSpeed, PmPeakMaxSpeed, PmPeakVariability, PmPeakPercentViolations, PmPeakPercentExtremeViolations, PmPeakAvgSpeedVsSpeedLimit, PmPeakEightyFifthSpeedVsSpeedLimit, PmPeakPercentObserved, " +
                $"WeekendPmPeakAverageSpeed, WeekendPmPeakAverageEightyFifthSpeed, WeekendPmPeakViolations, WeekendPmPeakExtremeViolations, WeekendPmPeakFlow, WeekendPmPeakMinSpeed, WeekendPmPeakMaxSpeed, WeekendPmPeakVariability, WeekendPmPeakPercentViolations, WeekendPmPeakPercentExtremeViolations, WeekendPmPeakAvgSpeedVsSpeedLimit, WeekendPmPeakEightyFifthSpeedVsSpeedLimit, WeekendPmPeakPercentObserved, " +
                $"WeekdayPmPeakAverageSpeed, WeekdayPmPeakAverageEightyFifthSpeed, WeekdayPmPeakViolations, WeekdayPmPeakExtremeViolations, WeekdayPmPeakFlow, WeekdayPmPeakMinSpeed, WeekdayPmPeakMaxSpeed, WeekdayPmPeakVariability, WeekdayPmPeakPercentViolations, WeekdayPmPeakPercentExtremeViolations, WeekdayPmPeakAvgSpeedVsSpeedLimit, WeekdayPmPeakEightyFifthSpeedVsSpeedLimit, WeekdayPmPeakPercentObserved, " +

                $"MidDayAverageSpeed, MidDayAverageEightyFifthSpeed, MidDayViolations, MidDayExtremeViolations, MidDayFlow, MidDayMinSpeed, MidDayMaxSpeed, MidDayVariability, MidDayPercentViolations, MidDayPercentExtremeViolations, MidDayAvgSpeedVsSpeedLimit, MidDayEightyFifthSpeedVsSpeedLimit, MidDayPercentObserved, " +
                $"WeekendMidDayAverageSpeed, WeekendMidDayAverageEightyFifthSpeed, WeekendMidDayViolations, WeekendMidDayExtremeViolations, WeekendMidDayFlow, WeekendMidDayMinSpeed, WeekendMidDayMaxSpeed, WeekendMidDayVariability, WeekendMidDayPercentViolations, WeekendMidDayPercentExtremeViolations, WeekendMidDayAvgSpeedVsSpeedLimit, WeekendMidDayEightyFifthSpeedVsSpeedLimit, WeekendMidDayPercentObserved, " +
                $"WeekdayMidDayAverageSpeed, WeekdayMidDayAverageEightyFifthSpeed, WeekdayMidDayViolations, WeekdayMidDayExtremeViolations, WeekdayMidDayFlow, WeekdayMidDayMinSpeed, WeekdayMidDayMaxSpeed, WeekdayMidDayVariability, WeekdayMidDayPercentViolations, WeekdayMidDayPercentExtremeViolations, WeekdayMidDayAvgSpeedVsSpeedLimit, WeekdayMidDayEightyFifthSpeedVsSpeedLimit, WeekdayMidDayPercentObserved, " +

                $"EveningAverageSpeed, EveningAverageEightyFifthSpeed, EveningViolations, EveningExtremeViolations, EveningFlow, EveningMinSpeed, EveningMaxSpeed, EveningVariability, EveningPercentViolations, EveningPercentExtremeViolations, EveningAvgSpeedVsSpeedLimit, EveningEightyFifthSpeedVsSpeedLimit, EveningPercentObserved, " +
                $"WeekendEveningAverageSpeed, WeekendEveningAverageEightyFifthSpeed, WeekendEveningViolations, WeekendEveningExtremeViolations, WeekendEveningFlow, WeekendEveningMinSpeed, WeekendEveningMaxSpeed, WeekendEveningVariability, WeekendEveningPercentViolations, WeekendEveningPercentExtremeViolations, WeekendEveningAvgSpeedVsSpeedLimit, WeekendEveningEightyFifthSpeedVsSpeedLimit, WeekendEveningPercentObserved, " +
                $"WeekdayEveningAverageSpeed, WeekdayEveningAverageEightyFifthSpeed, WeekdayEveningViolations, WeekdayEveningExtremeViolations, WeekdayEveningFlow, WeekdayEveningMinSpeed, WeekdayEveningMaxSpeed, WeekdayEveningVariability, WeekdayEveningPercentViolations, WeekdayEveningPercentExtremeViolations, WeekdayEveningAvgSpeedVsSpeedLimit, WeekdayEveningEightyFifthSpeedVsSpeedLimit, WeekdayEveningPercentObserved, " +

                $"EarlyMorningAverageSpeed, EarlyMorningAverageEightyFifthSpeed, EarlyMorningViolations, EarlyMorningExtremeViolations, EarlyMorningFlow, EarlyMorningMinSpeed, EarlyMorningMaxSpeed, EarlyMorningVariability, EarlyMorningPercentViolations, EarlyMorningPercentExtremeViolations, EarlyMorningAvgSpeedVsSpeedLimit, EarlyMorningEightyFifthSpeedVsSpeedLimit, EarlyMorningPercentObserved, " +
                $"WeekendEarlyMorningAverageSpeed, WeekendEarlyMorningAverageEightyFifthSpeed, WeekendEarlyMorningViolations, WeekendEarlyMorningExtremeViolations, WeekendEarlyMorningFlow, WeekendEarlyMorningMinSpeed, WeekendEarlyMorningMaxSpeed, WeekendEarlyMorningVariability, WeekendEarlyMorningPercentViolations, WeekendEarlyMorningPercentExtremeViolations, WeekendEarlyMorningAvgSpeedVsSpeedLimit, WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit, WeekendEarlyMorningPercentObserved, " +
                $"WeekdayEarlyMorningAverageSpeed, WeekdayEarlyMorningAverageEightyFifthSpeed, WeekdayEarlyMorningViolations, WeekdayEarlyMorningExtremeViolations, WeekdayEarlyMorningFlow, WeekdayEarlyMorningMinSpeed, WeekdayEarlyMorningMaxSpeed, WeekdayEarlyMorningVariability, WeekdayEarlyMorningPercentViolations, WeekdayEarlyMorningPercentExtremeViolations, WeekdayEarlyMorningAvgSpeedVsSpeedLimit, WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit, WeekdayEarlyMorningPercentObserved) " +

                $"VALUES (" +
                $"GENERATE_UUID(), " +
                $"CURRENT_TIMESTAMP(), " +
                $"TIMESTAMP('{item.BinStartTime:yyyy-MM-dd HH:mm:ss}'), " +
                $"'{item.SegmentId}', " +
                $"{item.SourceId}, " +

                $"{(item.AllDayAverageSpeed.HasValue ? ((double)item.AllDayAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayAverageEightyFifthSpeed.HasValue ? ((double)item.AllDayAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayViolations.HasValue ? item.AllDayViolations.Value.ToString() : "NULL")}, " +
                $"{(item.AllDayExtremeViolations.HasValue ? item.AllDayExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.AllDayFlow.HasValue ? item.AllDayFlow.Value.ToString() : "NULL")}, " +
                $"{(item.AllDayMinSpeed.HasValue ? ((double)item.AllDayMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayMaxSpeed.HasValue ? ((double)item.AllDayMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayVariability.HasValue ? ((double)item.AllDayVariability.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayPercentViolations.HasValue ? ((double)item.AllDayPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayPercentExtremeViolations.HasValue ? ((double)item.AllDayPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayAvgSpeedVsSpeedLimit.HasValue ? ((double)item.AllDayAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.AllDayEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.AllDayPercentObserved.HasValue ? ((double)item.AllDayPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekendAllDayAverageSpeed.HasValue ? ((double)item.WeekendAllDayAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayAverageEightyFifthSpeed.HasValue ? ((double)item.WeekendAllDayAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayViolations.HasValue ? item.WeekendAllDayViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayExtremeViolations.HasValue ? item.WeekendAllDayExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayFlow.HasValue ? item.WeekendAllDayFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayMinSpeed.HasValue ? ((double)item.WeekendAllDayMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayMaxSpeed.HasValue ? ((double)item.WeekendAllDayMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayVariability.HasValue ? ((double)item.WeekendAllDayVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayPercentViolations.HasValue ? ((double)item.WeekendAllDayPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayPercentExtremeViolations.HasValue ? ((double)item.WeekendAllDayPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendAllDayAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendAllDayEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAllDayPercentObserved.HasValue ? ((double)item.WeekendAllDayPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekdayAllDayAverageSpeed.HasValue ? ((double)item.WeekdayAllDayAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayAverageEightyFifthSpeed.HasValue ? ((double)item.WeekdayAllDayAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayViolations.HasValue ? item.WeekdayAllDayViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayExtremeViolations.HasValue ? item.WeekdayAllDayExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayFlow.HasValue ? item.WeekdayAllDayFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayMinSpeed.HasValue ? ((double)item.WeekdayAllDayMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayMaxSpeed.HasValue ? ((double)item.WeekdayAllDayMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayVariability.HasValue ? ((double)item.WeekdayAllDayVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayPercentViolations.HasValue ? ((double)item.WeekdayAllDayPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayPercentExtremeViolations.HasValue ? ((double)item.WeekdayAllDayPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayAllDayAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayAllDayEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAllDayPercentObserved.HasValue ? ((double)item.WeekdayAllDayPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.OffPeakAverageSpeed.HasValue ? ((int)item.OffPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakAverageEightyFifthSpeed.HasValue ? ((int)item.OffPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakViolations.HasValue ? item.OffPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.OffPeakExtremeViolations.HasValue ? item.OffPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.OffPeakFlow.HasValue ? item.OffPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.OffPeakMinSpeed.HasValue ? ((double)item.OffPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakMaxSpeed.HasValue ? ((double)item.OffPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakVariability.HasValue ? ((double)item.OffPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakPercentViolations.HasValue ? ((double)item.OffPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakPercentExtremeViolations.HasValue ? ((double)item.OffPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.OffPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.OffPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.OffPeakPercentObserved.HasValue ? ((double)item.OffPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekendOffPeakAverageSpeed.HasValue ? ((double)item.WeekendOffPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakAverageEightyFifthSpeed.HasValue ? ((double)item.WeekendOffPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakViolations.HasValue ? item.WeekendOffPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakExtremeViolations.HasValue ? item.WeekendOffPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakFlow.HasValue ? item.WeekendOffPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakMinSpeed.HasValue ? ((double)item.WeekendOffPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakMaxSpeed.HasValue ? ((double)item.WeekendOffPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakVariability.HasValue ? ((double)item.WeekendOffPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakPercentViolations.HasValue ? ((double)item.WeekendOffPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakPercentExtremeViolations.HasValue ? ((double)item.WeekendOffPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendOffPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendOffPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendOffPeakPercentObserved.HasValue ? ((double)item.WeekendOffPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekdayOffPeakAverageSpeed.HasValue ? ((double)item.WeekdayOffPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakAverageEightyFifthSpeed.HasValue ? ((double)item.WeekdayOffPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakViolations.HasValue ? item.WeekdayOffPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakExtremeViolations.HasValue ? item.WeekdayOffPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakFlow.HasValue ? item.WeekdayOffPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakMinSpeed.HasValue ? ((double)item.WeekdayOffPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakMaxSpeed.HasValue ? ((double)item.WeekdayOffPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakVariability.HasValue ? ((double)item.WeekdayOffPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakPercentViolations.HasValue ? ((double)item.WeekdayOffPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakPercentExtremeViolations.HasValue ? ((double)item.WeekdayOffPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayOffPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayOffPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayOffPeakPercentObserved.HasValue ? ((double)item.WeekdayOffPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.AmPeakAverageSpeed.HasValue ? ((int)item.AmPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakAverageEightyFifthSpeed.HasValue ? ((int)item.AmPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakViolations.HasValue ? item.AmPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.AmPeakExtremeViolations.HasValue ? item.AmPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.AmPeakFlow.HasValue ? item.AmPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.AmPeakMinSpeed.HasValue ? ((double)item.AmPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakMaxSpeed.HasValue ? ((double)item.AmPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakVariability.HasValue ? ((double)item.AmPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakPercentViolations.HasValue ? ((double)item.AmPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakPercentExtremeViolations.HasValue ? ((double)item.AmPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.AmPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.AmPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.AmPeakPercentObserved.HasValue ? ((double)item.AmPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekendAmPeakAverageSpeed.HasValue ? ((double)item.WeekendAmPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakAverageEightyFifthSpeed.HasValue ? ((double)item.WeekendAmPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakViolations.HasValue ? item.WeekendAmPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakExtremeViolations.HasValue ? item.WeekendAmPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakFlow.HasValue ? item.WeekendAmPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakMinSpeed.HasValue ? ((double)item.WeekendAmPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakMaxSpeed.HasValue ? ((double)item.WeekendAmPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakVariability.HasValue ? ((double)item.WeekendAmPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakPercentViolations.HasValue ? ((double)item.WeekendAmPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakPercentExtremeViolations.HasValue ? ((double)item.WeekendAmPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendAmPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendAmPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendAmPeakPercentObserved.HasValue ? ((double)item.WeekendAmPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekdayAmPeakAverageSpeed.HasValue ? ((double)item.WeekdayAmPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakAverageEightyFifthSpeed.HasValue ? ((double)item.WeekdayAmPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakViolations.HasValue ? item.WeekdayAmPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakExtremeViolations.HasValue ? item.WeekdayAmPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakFlow.HasValue ? item.WeekdayAmPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakMinSpeed.HasValue ? ((double)item.WeekdayAmPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakMaxSpeed.HasValue ? ((double)item.WeekdayAmPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakVariability.HasValue ? ((double)item.WeekdayAmPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakPercentViolations.HasValue ? ((double)item.WeekdayAmPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakPercentExtremeViolations.HasValue ? ((double)item.WeekdayAmPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayAmPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayAmPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayAmPeakPercentObserved.HasValue ? ((double)item.WeekdayAmPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.PmPeakAverageSpeed.HasValue ? ((int)item.PmPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakAverageEightyFifthSpeed.HasValue ? ((int)item.PmPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakViolations.HasValue ? item.PmPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.PmPeakExtremeViolations.HasValue ? item.PmPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.PmPeakFlow.HasValue ? item.PmPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.PmPeakMinSpeed.HasValue ? ((double)item.PmPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakMaxSpeed.HasValue ? ((double)item.PmPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakVariability.HasValue ? ((double)item.PmPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakPercentViolations.HasValue ? ((double)item.PmPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakPercentExtremeViolations.HasValue ? ((double)item.PmPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.PmPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.PmPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.PmPeakPercentObserved.HasValue ? ((double)item.PmPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekendPmPeakAverageSpeed.HasValue ? ((double)item.WeekendPmPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakAverageEightyFifthSpeed.HasValue ? ((double)item.WeekendPmPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakViolations.HasValue ? item.WeekendPmPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakExtremeViolations.HasValue ? item.WeekendPmPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakFlow.HasValue ? item.WeekendPmPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakMinSpeed.HasValue ? ((double)item.WeekendPmPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakMaxSpeed.HasValue ? ((double)item.WeekendPmPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakVariability.HasValue ? ((double)item.WeekendPmPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakPercentViolations.HasValue ? ((double)item.WeekendPmPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakPercentExtremeViolations.HasValue ? ((double)item.WeekendPmPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendPmPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendPmPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendPmPeakPercentObserved.HasValue ? ((double)item.WeekendPmPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekdayPmPeakAverageSpeed.HasValue ? ((double)item.WeekdayPmPeakAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakAverageEightyFifthSpeed.HasValue ? ((double)item.WeekdayPmPeakAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakViolations.HasValue ? item.WeekdayPmPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakExtremeViolations.HasValue ? item.WeekdayPmPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakFlow.HasValue ? item.WeekdayPmPeakFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakMinSpeed.HasValue ? ((double)item.WeekdayPmPeakMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakMaxSpeed.HasValue ? ((double)item.WeekdayPmPeakMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakVariability.HasValue ? ((double)item.WeekdayPmPeakVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakPercentViolations.HasValue ? ((double)item.WeekdayPmPeakPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakPercentExtremeViolations.HasValue ? ((double)item.WeekdayPmPeakPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayPmPeakAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayPmPeakEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayPmPeakPercentObserved.HasValue ? ((double)item.WeekdayPmPeakPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.MidDayAverageSpeed.HasValue ? ((int)item.MidDayAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayAverageEightyFifthSpeed.HasValue ? ((int)item.MidDayAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayViolations.HasValue ? item.MidDayViolations.Value.ToString() : "NULL")}, " +
                $"{(item.MidDayExtremeViolations.HasValue ? item.MidDayExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.MidDayFlow.HasValue ? item.MidDayFlow.Value.ToString() : "NULL")}, " +
                $"{(item.MidDayMinSpeed.HasValue ? ((double)item.MidDayMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayMaxSpeed.HasValue ? ((double)item.MidDayMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayVariability.HasValue ? ((double)item.MidDayVariability.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayPercentViolations.HasValue ? ((double)item.MidDayPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayPercentExtremeViolations.HasValue ? ((double)item.MidDayPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayAvgSpeedVsSpeedLimit.HasValue ? ((double)item.MidDayAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.MidDayEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.MidDayPercentObserved.HasValue ? ((double)item.MidDayPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekendMidDayAverageSpeed.HasValue ? ((double)item.WeekendMidDayAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayAverageEightyFifthSpeed.HasValue ? ((double)item.WeekendMidDayAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayViolations.HasValue ? item.WeekendMidDayViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayExtremeViolations.HasValue ? item.WeekendMidDayExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayFlow.HasValue ? item.WeekendMidDayFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayMinSpeed.HasValue ? ((double)item.WeekendMidDayMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayMaxSpeed.HasValue ? ((double)item.WeekendMidDayMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayVariability.HasValue ? ((double)item.WeekendMidDayVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayPercentViolations.HasValue ? ((double)item.WeekendMidDayPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayPercentExtremeViolations.HasValue ? ((double)item.WeekendMidDayPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendMidDayAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendMidDayEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendMidDayPercentObserved.HasValue ? ((double)item.WeekendMidDayPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekdayMidDayAverageSpeed.HasValue ? ((double)item.WeekdayMidDayAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayAverageEightyFifthSpeed.HasValue ? ((double)item.WeekdayMidDayAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayViolations.HasValue ? item.WeekdayMidDayViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayExtremeViolations.HasValue ? item.WeekdayMidDayExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayFlow.HasValue ? item.WeekdayMidDayFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayMinSpeed.HasValue ? ((double)item.WeekdayMidDayMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayMaxSpeed.HasValue ? ((double)item.WeekdayMidDayMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayVariability.HasValue ? ((double)item.WeekdayMidDayVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayPercentViolations.HasValue ? ((double)item.WeekdayMidDayPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayPercentExtremeViolations.HasValue ? ((double)item.WeekdayMidDayPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayMidDayAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayMidDayEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayMidDayPercentObserved.HasValue ? ((double)item.WeekdayMidDayPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.EveningAverageSpeed.HasValue ? ((int)item.EveningAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.EveningAverageEightyFifthSpeed.HasValue ? ((int)item.EveningAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.EveningViolations.HasValue ? item.EveningViolations.Value.ToString() : "NULL")}, " +
                $"{(item.EveningExtremeViolations.HasValue ? item.EveningExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.EveningFlow.HasValue ? item.EveningFlow.Value.ToString() : "NULL")}, " +
                $"{(item.EveningMinSpeed.HasValue ? ((double)item.EveningMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.EveningMaxSpeed.HasValue ? ((double)item.EveningMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.EveningVariability.HasValue ? ((double)item.EveningVariability.Value).ToString() : "NULL")}, " +
                $"{(item.EveningPercentViolations.HasValue ? ((double)item.EveningPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.EveningPercentExtremeViolations.HasValue ? ((double)item.EveningPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.EveningAvgSpeedVsSpeedLimit.HasValue ? ((double)item.EveningAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.EveningEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.EveningEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.EveningPercentObserved.HasValue ? ((double)item.EveningPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekendEveningAverageSpeed.HasValue ? ((double)item.WeekendEveningAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningAverageEightyFifthSpeed.HasValue ? ((double)item.WeekendEveningAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningViolations.HasValue ? item.WeekendEveningViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendEveningExtremeViolations.HasValue ? item.WeekendEveningExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendEveningFlow.HasValue ? item.WeekendEveningFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendEveningMinSpeed.HasValue ? ((double)item.WeekendEveningMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningMaxSpeed.HasValue ? ((double)item.WeekendEveningMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningVariability.HasValue ? ((double)item.WeekendEveningVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningPercentViolations.HasValue ? ((double)item.WeekendEveningPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningPercentExtremeViolations.HasValue ? ((double)item.WeekendEveningPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendEveningAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendEveningEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEveningPercentObserved.HasValue ? ((double)item.WeekendEveningPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekdayEveningAverageSpeed.HasValue ? ((double)item.WeekdayEveningAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningAverageEightyFifthSpeed.HasValue ? ((double)item.WeekdayEveningAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningViolations.HasValue ? item.WeekdayEveningViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningExtremeViolations.HasValue ? item.WeekdayEveningExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningFlow.HasValue ? item.WeekdayEveningFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningMinSpeed.HasValue ? ((double)item.WeekdayEveningMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningMaxSpeed.HasValue ? ((double)item.WeekdayEveningMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningVariability.HasValue ? ((double)item.WeekdayEveningVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningPercentViolations.HasValue ? ((double)item.WeekdayEveningPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningPercentExtremeViolations.HasValue ? ((double)item.WeekdayEveningPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayEveningAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayEveningEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEveningPercentObserved.HasValue ? ((double)item.WeekdayEveningPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.EarlyMorningAverageSpeed.HasValue ? ((int)item.EarlyMorningAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningAverageEightyFifthSpeed.HasValue ? ((int)item.EarlyMorningAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningViolations.HasValue ? item.EarlyMorningViolations.Value.ToString() : "NULL")}, " +
                $"{(item.EarlyMorningExtremeViolations.HasValue ? item.EarlyMorningExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.EarlyMorningFlow.HasValue ? item.EarlyMorningFlow.Value.ToString() : "NULL")}, " +
                $"{(item.EarlyMorningMinSpeed.HasValue ? ((double)item.EarlyMorningMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningMaxSpeed.HasValue ? ((double)item.EarlyMorningMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningVariability.HasValue ? ((double)item.EarlyMorningVariability.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningPercentViolations.HasValue ? ((double)item.EarlyMorningPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningPercentExtremeViolations.HasValue ? ((double)item.EarlyMorningPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningAvgSpeedVsSpeedLimit.HasValue ? ((double)item.EarlyMorningAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.EarlyMorningEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.EarlyMorningPercentObserved.HasValue ? ((double)item.EarlyMorningPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekendEarlyMorningAverageSpeed.HasValue ? ((double)item.WeekendEarlyMorningAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningAverageEightyFifthSpeed.HasValue ? ((double)item.WeekendEarlyMorningAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningViolations.HasValue ? item.WeekendEarlyMorningViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningExtremeViolations.HasValue ? item.WeekendEarlyMorningExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningFlow.HasValue ? item.WeekendEarlyMorningFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningMinSpeed.HasValue ? ((double)item.WeekendEarlyMorningMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningMaxSpeed.HasValue ? ((double)item.WeekendEarlyMorningMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningVariability.HasValue ? ((double)item.WeekendEarlyMorningVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningPercentViolations.HasValue ? ((double)item.WeekendEarlyMorningPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningPercentExtremeViolations.HasValue ? ((double)item.WeekendEarlyMorningPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendEarlyMorningAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekendEarlyMorningPercentObserved.HasValue ? ((double)item.WeekendEarlyMorningPercentObserved.Value).ToString() : "NULL")}, " +

                $"{(item.WeekdayEarlyMorningAverageSpeed.HasValue ? ((double)item.WeekdayEarlyMorningAverageSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningAverageEightyFifthSpeed.HasValue ? ((double)item.WeekdayEarlyMorningAverageEightyFifthSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningViolations.HasValue ? item.WeekdayEarlyMorningViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningExtremeViolations.HasValue ? item.WeekdayEarlyMorningExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningFlow.HasValue ? item.WeekdayEarlyMorningFlow.Value.ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningMinSpeed.HasValue ? ((double)item.WeekdayEarlyMorningMinSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningMaxSpeed.HasValue ? ((double)item.WeekdayEarlyMorningMaxSpeed.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningVariability.HasValue ? ((double)item.WeekdayEarlyMorningVariability.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningPercentViolations.HasValue ? ((double)item.WeekdayEarlyMorningPercentViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningPercentExtremeViolations.HasValue ? ((double)item.WeekdayEarlyMorningPercentExtremeViolations.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningAvgSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayEarlyMorningAvgSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit.HasValue ? ((double)item.WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit.Value).ToString() : "NULL")}, " +
                $"{(item.WeekdayEarlyMorningPercentObserved.HasValue ? ((double)item.WeekdayEarlyMorningPercentObserved.Value).ToString() : "NULL")}";
        }

        private string updateQuery(MonthlyAggregation item)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");
            queryBuilder.Append($"BinStartTime = TIMESTAMP('{item.BinStartTime:yyyy-MM-dd HH:mm:ss}'), ");
            queryBuilder.Append($"SegmentId = '{item.SegmentId}', ");
            queryBuilder.Append($"SourceId = {item.SourceId}, ");

            ////////////////////////
            ///////ALL DAY ////////
            ////////////////////////

            if (item.AllDayAverageSpeed.HasValue)
            {
                queryBuilder.Append($"AllDayAverageSpeed = {item.AllDayAverageSpeed.Value}, ");
            }
            if (item.AllDayAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"AllDayAverageEightyFifthSpeed = {item.AllDayAverageEightyFifthSpeed.Value}, ");
            }

            if (item.AllDayViolations.HasValue)
            {
                queryBuilder.Append($"AllDayViolations = {item.AllDayViolations.Value}, ");
            }

            if (item.AllDayExtremeViolations.HasValue)
            {
                queryBuilder.Append($"AllDayExtremeViolations = {item.AllDayExtremeViolations.Value}, ");
            }

            if (item.AllDayFlow.HasValue)
            {
                queryBuilder.Append($"AllDayFlow = {item.AllDayFlow.Value}, ");
            }

            if (item.AllDayMinSpeed.HasValue)
            {
                queryBuilder.Append($"AllDayMinSpeed = {item.AllDayMinSpeed.Value}, ");
            }

            if (item.AllDayMaxSpeed.HasValue)
            {
                queryBuilder.Append($"AllDayMaxSpeed = {item.AllDayMaxSpeed.Value}, ");
            }

            if (item.AllDayVariability.HasValue)
            {
                queryBuilder.Append($"AllDayVariability = {item.AllDayVariability.Value}, ");
            }

            if (item.AllDayPercentViolations.HasValue)
            {
                queryBuilder.Append($"AllDayPercentViolations = {item.AllDayPercentViolations.Value}, ");
            }

            if (item.AllDayPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"AllDayPercentExtremeViolations = {item.AllDayPercentExtremeViolations.Value}, ");
            }

            if (item.AllDayAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"AllDayAvgSpeedVsSpeedLimit = {item.AllDayAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.AllDayEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"AllDayEightyFifthSpeedVsSpeedLimit = {item.AllDayEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.AllDayPercentObserved.HasValue)
            {
                queryBuilder.Append($"AllDayPercentObserved = {item.AllDayPercentObserved.Value}, ");
            }

            //////WEEKEND//////
            if (item.WeekendAllDayAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayAverageSpeed = {item.WeekendAllDayAverageSpeed.Value}, ");
            }
            if (item.WeekendAllDayAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayAverageEightyFifthSpeed = {item.WeekendAllDayAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekendAllDayViolations.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayViolations = {item.WeekendAllDayViolations.Value}, ");
            }

            if (item.WeekendAllDayExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayExtremeViolations = {item.WeekendAllDayExtremeViolations.Value}, ");
            }

            if (item.WeekendAllDayFlow.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayFlow = {item.WeekendAllDayFlow.Value}, ");
            }

            if (item.WeekendAllDayMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayMinSpeed = {item.WeekendAllDayMinSpeed.Value}, ");
            }

            if (item.WeekendAllDayMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayMaxSpeed = {item.WeekendAllDayMaxSpeed.Value}, ");
            }

            if (item.WeekendAllDayVariability.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayVariability = {item.WeekendAllDayVariability.Value}, ");
            }

            if (item.WeekendAllDayPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayPercentViolations = {item.WeekendAllDayPercentViolations.Value}, ");
            }

            if (item.WeekendAllDayPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayPercentExtremeViolations = {item.WeekendAllDayPercentExtremeViolations.Value}, ");
            }

            if (item.WeekendAllDayAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayAvgSpeedVsSpeedLimit = {item.WeekendAllDayAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekendAllDayEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayEightyFifthSpeedVsSpeedLimit = {item.WeekendAllDayEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekendAllDayPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekendAllDayPercentObserved = {item.WeekendAllDayPercentObserved.Value}, ");
            }

            //////WEEKDAY//////

            if (item.WeekdayAllDayAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayAverageSpeed = {item.WeekdayAllDayAverageSpeed.Value}, ");
            }
            if (item.WeekdayAllDayAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayAverageEightyFifthSpeed = {item.WeekdayAllDayAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekdayAllDayViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayViolations = {item.WeekdayAllDayViolations.Value}, ");
            }

            if (item.WeekdayAllDayExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayExtremeViolations = {item.WeekdayAllDayExtremeViolations.Value}, ");
            }

            if (item.WeekdayAllDayFlow.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayFlow = {item.WeekdayAllDayFlow.Value}, ");
            }

            if (item.WeekdayAllDayMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayMinSpeed = {item.WeekdayAllDayMinSpeed.Value}, ");
            }

            if (item.WeekdayAllDayMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayMaxSpeed = {item.WeekdayAllDayMaxSpeed.Value}, ");
            }

            if (item.WeekdayAllDayVariability.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayVariability = {item.WeekdayAllDayVariability.Value}, ");
            }

            if (item.WeekdayAllDayPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayPercentViolations = {item.WeekdayAllDayPercentViolations.Value}, ");
            }

            if (item.WeekdayAllDayPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayPercentExtremeViolations = {item.WeekdayAllDayPercentExtremeViolations.Value}, ");
            }

            if (item.WeekdayAllDayAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayAvgSpeedVsSpeedLimit = {item.WeekdayAllDayAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekdayAllDayEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayEightyFifthSpeedVsSpeedLimit = {item.WeekdayAllDayEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekdayAllDayPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekdayAllDayPercentObserved = {item.WeekdayAllDayPercentObserved.Value}, ");
            }

            ////////////////////////
            ///////OFF PEAK ////////
            ////////////////////////

            if (item.OffPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"OffPeakAverageSpeed = {item.OffPeakAverageSpeed.Value}, ");
            }

            if (item.OffPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"OffPeakAverageEightyFifthSpeed = {item.OffPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.OffPeakViolations.HasValue)
            {
                queryBuilder.Append($"OffPeakViolations = {item.OffPeakViolations.Value}, ");
            }

            if (item.OffPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"OffPeakExtremeViolations = {item.OffPeakExtremeViolations.Value}, ");
            }

            if (item.OffPeakFlow.HasValue)
            {
                queryBuilder.Append($"OffPeakFlow = {item.OffPeakFlow.Value}, ");
            }

            if (item.OffPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"OffPeakMinSpeed = {item.OffPeakMinSpeed.Value}, ");
            }

            if (item.OffPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"OffPeakMaxSpeed = {item.OffPeakMaxSpeed.Value}, ");
            }

            if (item.OffPeakVariability.HasValue)
            {
                queryBuilder.Append($"OffPeakVariability = {item.OffPeakVariability.Value}, ");
            }

            if (item.OffPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"OffPeakPercentViolations = {item.OffPeakPercentViolations.Value}, ");
            }

            if (item.OffPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"OffPeakPercentExtremeViolations = {item.OffPeakPercentExtremeViolations.Value}, ");
            }

            if (item.OffPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"OffPeakAvgSpeedVsSpeedLimit = {item.OffPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.OffPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"OffPeakEightyFifthSpeedVsSpeedLimit = {item.OffPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.OffPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"OffPeakPercentObserved = {item.OffPeakPercentObserved.Value}, ");
            }

            //////WEEKEND//////
            if (item.WeekendOffPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakAverageSpeed = {item.WeekendOffPeakAverageSpeed.Value}, ");
            }
            if (item.WeekendOffPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakAverageEightyFifthSpeed = {item.WeekendOffPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekendOffPeakViolations.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakViolations = {item.WeekendOffPeakViolations.Value}, ");
            }

            if (item.WeekendOffPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakExtremeViolations = {item.WeekendOffPeakExtremeViolations.Value}, ");
            }

            if (item.WeekendOffPeakFlow.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakFlow = {item.WeekendOffPeakFlow.Value}, ");
            }

            if (item.WeekendOffPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakMinSpeed = {item.WeekendOffPeakMinSpeed.Value}, ");
            }

            if (item.WeekendOffPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakMaxSpeed = {item.WeekendOffPeakMaxSpeed.Value}, ");
            }

            if (item.WeekendOffPeakVariability.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakVariability = {item.WeekendOffPeakVariability.Value}, ");
            }

            if (item.WeekendOffPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakPercentViolations = {item.WeekendOffPeakPercentViolations.Value}, ");
            }

            if (item.WeekendOffPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakPercentExtremeViolations = {item.WeekendOffPeakPercentExtremeViolations.Value}, ");
            }

            if (item.WeekendOffPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakAvgSpeedVsSpeedLimit = {item.WeekendOffPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekendOffPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakEightyFifthSpeedVsSpeedLimit = {item.WeekendOffPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekendOffPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekendOffPeakPercentObserved = {item.WeekendOffPeakPercentObserved.Value}, ");
            }

            //////WEEKDAY//////
            if (item.WeekdayOffPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakAverageSpeed = {item.WeekdayOffPeakAverageSpeed.Value}, ");
            }
            if (item.WeekdayOffPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakAverageEightyFifthSpeed = {item.WeekdayOffPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekdayOffPeakViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakViolations = {item.WeekdayOffPeakViolations.Value}, ");
            }

            if (item.WeekdayOffPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakExtremeViolations = {item.WeekdayOffPeakExtremeViolations.Value}, ");
            }

            if (item.WeekdayOffPeakFlow.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakFlow = {item.WeekdayOffPeakFlow.Value}, ");
            }

            if (item.WeekdayOffPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakMinSpeed = {item.WeekdayOffPeakMinSpeed.Value}, ");
            }

            if (item.WeekdayOffPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakMaxSpeed = {item.WeekdayOffPeakMaxSpeed.Value}, ");
            }

            if (item.WeekdayOffPeakVariability.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakVariability = {item.WeekdayOffPeakVariability.Value}, ");
            }

            if (item.WeekdayOffPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakPercentViolations = {item.WeekdayOffPeakPercentViolations.Value}, ");
            }

            if (item.WeekdayOffPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakPercentExtremeViolations = {item.WeekdayOffPeakPercentExtremeViolations.Value}, ");
            }

            if (item.WeekdayOffPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakAvgSpeedVsSpeedLimit = {item.WeekdayOffPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekdayOffPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakEightyFifthSpeedVsSpeedLimit = {item.WeekdayOffPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekdayOffPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekdayOffPeakPercentObserved = {item.WeekdayOffPeakPercentObserved.Value}, ");
            }


            ////////////////////////
            ///////AM PEAK ////////
            ////////////////////////

            if (item.AmPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"AmPeakAverageSpeed = {item.AmPeakAverageSpeed.Value}, ");
            }

            if (item.AmPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"AmPeakAverageEightyFifthSpeed = {item.AmPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.AmPeakViolations.HasValue)
            {
                queryBuilder.Append($"AmPeakViolations = {item.AmPeakViolations.Value}, ");
            }

            if (item.AmPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"AmPeakExtremeViolations = {item.AmPeakExtremeViolations.Value}, ");
            }

            if (item.AmPeakFlow.HasValue)
            {
                queryBuilder.Append($"AmPeakFlow = {item.AmPeakFlow.Value}, ");
            }

            if (item.AmPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"AmPeakMinSpeed = {item.AmPeakMinSpeed.Value}, ");
            }

            if (item.AmPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"AmPeakMaxSpeed = {item.AmPeakMaxSpeed.Value}, ");
            }

            if (item.AmPeakVariability.HasValue)
            {
                queryBuilder.Append($"AmPeakVariability = {item.AmPeakVariability.Value}, ");
            }

            if (item.AmPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"AmPeakPercentViolations = {item.AmPeakPercentViolations.Value}, ");
            }

            if (item.AmPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"AmPeakPercentExtremeViolations = {item.AmPeakPercentExtremeViolations.Value}, ");
            }

            if (item.AmPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"AmPeakAvgSpeedVsSpeedLimit = {item.AmPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.AmPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"AmPeakEightyFifthSpeedVsSpeedLimit = {item.AmPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.AmPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"AmPeakPercentObserved = {item.AmPeakPercentObserved.Value}, ");
            }

            //////WEEKEND//////
            if (item.WeekendAmPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakAverageSpeed = {item.WeekendAmPeakAverageSpeed.Value}, ");
            }
            if (item.WeekendAmPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakAverageEightyFifthSpeed = {item.WeekendAmPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekendAmPeakViolations.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakViolations = {item.WeekendAmPeakViolations.Value}, ");
            }

            if (item.WeekendAmPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakExtremeViolations = {item.WeekendAmPeakExtremeViolations.Value}, ");
            }

            if (item.WeekendAmPeakFlow.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakFlow = {item.WeekendAmPeakFlow.Value}, ");
            }

            if (item.WeekendAmPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakMinSpeed = {item.WeekendAmPeakMinSpeed.Value}, ");
            }

            if (item.WeekendAmPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakMaxSpeed = {item.WeekendAmPeakMaxSpeed.Value}, ");
            }

            if (item.WeekendAmPeakVariability.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakVariability = {item.WeekendAmPeakVariability.Value}, ");
            }

            if (item.WeekendAmPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakPercentViolations = {item.WeekendAmPeakPercentViolations.Value}, ");
            }

            if (item.WeekendAmPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakPercentExtremeViolations = {item.WeekendAmPeakPercentExtremeViolations.Value}, ");
            }

            if (item.WeekendAmPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakAvgSpeedVsSpeedLimit = {item.WeekendAmPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekendAmPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakEightyFifthSpeedVsSpeedLimit = {item.WeekendAmPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekendAmPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekendAmPeakPercentObserved = {item.WeekendAmPeakPercentObserved.Value}, ");
            }

            //////WEEKDAY//////
            if (item.WeekdayAmPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakAverageSpeed = {item.WeekdayAmPeakAverageSpeed.Value}, ");
            }
            if (item.WeekdayAmPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakAverageEightyFifthSpeed = {item.WeekdayAmPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekdayAmPeakViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakViolations = {item.WeekdayAmPeakViolations.Value}, ");
            }

            if (item.WeekdayAmPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakExtremeViolations = {item.WeekdayAmPeakExtremeViolations.Value}, ");
            }

            if (item.WeekdayAmPeakFlow.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakFlow = {item.WeekdayAmPeakFlow.Value}, ");
            }

            if (item.WeekdayAmPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakMinSpeed = {item.WeekdayAmPeakMinSpeed.Value}, ");
            }

            if (item.WeekdayAmPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakMaxSpeed = {item.WeekdayAmPeakMaxSpeed.Value}, ");
            }

            if (item.WeekdayAmPeakVariability.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakVariability = {item.WeekdayAmPeakVariability.Value}, ");
            }

            if (item.WeekdayAmPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakPercentViolations = {item.WeekdayAmPeakPercentViolations.Value}, ");
            }

            if (item.WeekdayAmPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakPercentExtremeViolations = {item.WeekdayAmPeakPercentExtremeViolations.Value}, ");
            }

            if (item.WeekdayAmPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakAvgSpeedVsSpeedLimit = {item.WeekdayAmPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekdayAmPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakEightyFifthSpeedVsSpeedLimit = {item.WeekdayAmPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekdayAmPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekdayAmPeakPercentObserved = {item.WeekdayAmPeakPercentObserved.Value}, ");
            }

            ////////////////////////
            ///////PM PEAK ////////
            ////////////////////////

            if (item.PmPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"PmPeakAverageSpeed = {item.PmPeakAverageSpeed.Value}, ");
            }

            if (item.PmPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"PmPeakAverageEightyFifthSpeed = {item.PmPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.PmPeakViolations.HasValue)
            {
                queryBuilder.Append($"PmPeakViolations = {item.PmPeakViolations.Value}, ");
            }

            if (item.PmPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"PmPeakExtremeViolations = {item.PmPeakExtremeViolations.Value}, ");
            }

            if (item.PmPeakFlow.HasValue)
            {
                queryBuilder.Append($"PmPeakFlow = {item.PmPeakFlow.Value}, ");
            }

            if (item.PmPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"PmPeakMinSpeed = {item.PmPeakMinSpeed.Value}, ");
            }

            if (item.PmPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"PmPeakMaxSpeed = {item.PmPeakMaxSpeed.Value}, ");
            }

            if (item.PmPeakVariability.HasValue)
            {
                queryBuilder.Append($"PmPeakVariability = {item.PmPeakVariability.Value}, ");
            }

            if (item.PmPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"PmPeakPercentViolations = {item.PmPeakPercentViolations.Value}, ");
            }

            if (item.PmPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"PmPeakPercentExtremeViolations = {item.PmPeakPercentExtremeViolations.Value}, ");
            }

            if (item.PmPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"PmPeakAvgSpeedVsSpeedLimit = {item.PmPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.PmPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"PmPeakEightyFifthSpeedVsSpeedLimit = {item.PmPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.PmPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"PmPeakPercentObserved = {item.PmPeakPercentObserved.Value}, ");
            }

            //////WEEKEND//////
            if (item.WeekendPmPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakAverageSpeed = {item.WeekendPmPeakAverageSpeed.Value}, ");
            }
            if (item.WeekendPmPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakAverageEightyFifthSpeed = {item.WeekendPmPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekendPmPeakViolations.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakViolations = {item.WeekendPmPeakViolations.Value}, ");
            }

            if (item.WeekendPmPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakExtremeViolations = {item.WeekendPmPeakExtremeViolations.Value}, ");
            }

            if (item.WeekendPmPeakFlow.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakFlow = {item.WeekendPmPeakFlow.Value}, ");
            }

            if (item.WeekendPmPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakMinSpeed = {item.WeekendPmPeakMinSpeed.Value}, ");
            }

            if (item.WeekendPmPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakMaxSpeed = {item.WeekendPmPeakMaxSpeed.Value}, ");
            }

            if (item.WeekendPmPeakVariability.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakVariability = {item.WeekendPmPeakVariability.Value}, ");
            }

            if (item.WeekendPmPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakPercentViolations = {item.WeekendPmPeakPercentViolations.Value}, ");
            }

            if (item.WeekendPmPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakPercentExtremeViolations = {item.WeekendPmPeakPercentExtremeViolations.Value}, ");
            }

            if (item.WeekendPmPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakAvgSpeedVsSpeedLimit = {item.WeekendPmPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekendPmPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakEightyFifthSpeedVsSpeedLimit = {item.WeekendPmPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekendPmPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekendPmPeakPercentObserved = {item.WeekendPmPeakPercentObserved.Value}, ");
            }

            //////WEEKDAY//////
            if (item.WeekdayPmPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakAverageSpeed = {item.WeekdayPmPeakAverageSpeed.Value}, ");
            }
            if (item.WeekdayPmPeakAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakAverageEightyFifthSpeed = {item.WeekdayPmPeakAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekdayPmPeakViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakViolations = {item.WeekdayPmPeakViolations.Value}, ");
            }

            if (item.WeekdayPmPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakExtremeViolations = {item.WeekdayPmPeakExtremeViolations.Value}, ");
            }

            if (item.WeekdayPmPeakFlow.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakFlow = {item.WeekdayPmPeakFlow.Value}, ");
            }

            if (item.WeekdayPmPeakMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakMinSpeed = {item.WeekdayPmPeakMinSpeed.Value}, ");
            }

            if (item.WeekdayPmPeakMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakMaxSpeed = {item.WeekdayPmPeakMaxSpeed.Value}, ");
            }

            if (item.WeekdayPmPeakVariability.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakVariability = {item.WeekdayPmPeakVariability.Value}, ");
            }

            if (item.WeekdayPmPeakPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakPercentViolations = {item.WeekdayPmPeakPercentViolations.Value}, ");
            }

            if (item.WeekdayPmPeakPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakPercentExtremeViolations = {item.WeekdayPmPeakPercentExtremeViolations.Value}, ");
            }

            if (item.WeekdayPmPeakAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakAvgSpeedVsSpeedLimit = {item.WeekdayPmPeakAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekdayPmPeakEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakEightyFifthSpeedVsSpeedLimit = {item.WeekdayPmPeakEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekdayPmPeakPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekdayPmPeakPercentObserved = {item.WeekdayPmPeakPercentObserved.Value}, ");
            }


            ////////////////////////
            ///////MID DAY ////////
            ////////////////////////

            if (item.MidDayAverageSpeed.HasValue)
            {
                queryBuilder.Append($"MidDayAverageSpeed = {item.MidDayAverageSpeed.Value}, ");
            }

            if (item.MidDayAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"MidDayAverageEightyFifthSpeed = {item.MidDayAverageEightyFifthSpeed.Value}, ");
            }

            if (item.MidDayViolations.HasValue)
            {
                queryBuilder.Append($"MidDayViolations = {item.MidDayViolations.Value}, ");
            }

            if (item.MidDayExtremeViolations.HasValue)
            {
                queryBuilder.Append($"MidDayExtremeViolations = {item.MidDayExtremeViolations.Value}, ");
            }

            if (item.MidDayFlow.HasValue)
            {
                queryBuilder.Append($"MidDayFlow = {item.MidDayFlow.Value}, ");
            }

            if (item.MidDayMinSpeed.HasValue)
            {
                queryBuilder.Append($"MidDayMinSpeed = {item.MidDayMinSpeed.Value}, ");
            }

            if (item.MidDayMaxSpeed.HasValue)
            {
                queryBuilder.Append($"MidDayMaxSpeed = {item.MidDayMaxSpeed.Value}, ");
            }

            if (item.MidDayVariability.HasValue)
            {
                queryBuilder.Append($"MidDayVariability = {item.MidDayVariability.Value}, ");
            }

            if (item.MidDayPercentViolations.HasValue)
            {
                queryBuilder.Append($"MidDayPercentViolations = {item.MidDayPercentViolations.Value}, ");
            }

            if (item.MidDayPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"MidDayPercentExtremeViolations = {item.MidDayPercentExtremeViolations.Value}, ");
            }

            if (item.MidDayAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"MidDayAvgSpeedVsSpeedLimit = {item.MidDayAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.MidDayEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"MidDayEightyFifthSpeedVsSpeedLimit = {item.MidDayEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.MidDayPercentObserved.HasValue)
            {
                queryBuilder.Append($"MidDayPercentObserved = {item.MidDayPercentObserved.Value}, ");
            }

            //////WEEKEND//////
            if (item.WeekendMidDayAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayAverageSpeed = {item.WeekendMidDayAverageSpeed.Value}, ");
            }
            if (item.WeekendMidDayAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayAverageEightyFifthSpeed = {item.WeekendMidDayAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekendMidDayViolations.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayViolations = {item.WeekendMidDayViolations.Value}, ");
            }

            if (item.WeekendMidDayExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayExtremeViolations = {item.WeekendMidDayExtremeViolations.Value}, ");
            }

            if (item.WeekendMidDayFlow.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayFlow = {item.WeekendMidDayFlow.Value}, ");
            }

            if (item.WeekendMidDayMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayMinSpeed = {item.WeekendMidDayMinSpeed.Value}, ");
            }

            if (item.WeekendMidDayMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayMaxSpeed = {item.WeekendMidDayMaxSpeed.Value}, ");
            }

            if (item.WeekendMidDayVariability.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayVariability = {item.WeekendMidDayVariability.Value}, ");
            }

            if (item.WeekendMidDayPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayPercentViolations = {item.WeekendMidDayPercentViolations.Value}, ");
            }

            if (item.WeekendMidDayPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayPercentExtremeViolations = {item.WeekendMidDayPercentExtremeViolations.Value}, ");
            }

            if (item.WeekendMidDayAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayAvgSpeedVsSpeedLimit = {item.WeekendMidDayAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekendMidDayEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayEightyFifthSpeedVsSpeedLimit = {item.WeekendMidDayEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekendMidDayPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekendMidDayPercentObserved = {item.WeekendMidDayPercentObserved.Value}, ");
            }

            //////WEEKDAY//////
            if (item.WeekdayMidDayAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayAverageSpeed = {item.WeekdayMidDayAverageSpeed.Value}, ");
            }
            if (item.WeekdayMidDayAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayAverageEightyFifthSpeed = {item.WeekdayMidDayAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekdayMidDayViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayViolations = {item.WeekdayMidDayViolations.Value}, ");
            }

            if (item.WeekdayMidDayExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayExtremeViolations = {item.WeekdayMidDayExtremeViolations.Value}, ");
            }

            if (item.WeekdayMidDayFlow.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayFlow = {item.WeekdayMidDayFlow.Value}, ");
            }

            if (item.WeekdayMidDayMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayMinSpeed = {item.WeekdayMidDayMinSpeed.Value}, ");
            }

            if (item.WeekdayMidDayMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayMaxSpeed = {item.WeekdayMidDayMaxSpeed.Value}, ");
            }

            if (item.WeekdayMidDayVariability.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayVariability = {item.WeekdayMidDayVariability.Value}, ");
            }

            if (item.WeekdayMidDayPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayPercentViolations = {item.WeekdayMidDayPercentViolations.Value}, ");
            }

            if (item.WeekdayMidDayPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayPercentExtremeViolations = {item.WeekdayMidDayPercentExtremeViolations.Value}, ");
            }

            if (item.WeekdayMidDayAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayAvgSpeedVsSpeedLimit = {item.WeekdayMidDayAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekdayMidDayEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayEightyFifthSpeedVsSpeedLimit = {item.WeekdayMidDayEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekdayMidDayPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekdayMidDayPercentObserved = {item.WeekdayMidDayPercentObserved.Value}, ");
            }


            ////////////////////////
            ///////EVENING ////////
            ////////////////////////

            if (item.EveningAverageSpeed.HasValue)
            {
                queryBuilder.Append($"EveningAverageSpeed = {item.EveningAverageSpeed.Value}, ");
            }

            if (item.EveningAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"EveningAverageEightyFifthSpeed = {item.EveningAverageEightyFifthSpeed.Value}, ");
            }

            if (item.EveningViolations.HasValue)
            {
                queryBuilder.Append($"EveningViolations = {item.EveningViolations.Value}, ");
            }

            if (item.EveningExtremeViolations.HasValue)
            {
                queryBuilder.Append($"EveningExtremeViolations = {item.EveningExtremeViolations.Value}, ");
            }

            if (item.EveningFlow.HasValue)
            {
                queryBuilder.Append($"EveningFlow = {item.EveningFlow.Value}, ");
            }

            if (item.EveningMinSpeed.HasValue)
            {
                queryBuilder.Append($"EveningMinSpeed = {item.EveningMinSpeed.Value}, ");
            }

            if (item.EveningMaxSpeed.HasValue)
            {
                queryBuilder.Append($"EveningMaxSpeed = {item.EveningMaxSpeed.Value}, ");
            }

            if (item.EveningVariability.HasValue)
            {
                queryBuilder.Append($"EveningVariability = {item.EveningVariability.Value}, ");
            }

            if (item.EveningPercentViolations.HasValue)
            {
                queryBuilder.Append($"EveningPercentViolations = {item.EveningPercentViolations.Value}, ");
            }

            if (item.EveningPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"EveningPercentExtremeViolations = {item.EveningPercentExtremeViolations.Value}, ");
            }

            if (item.EveningAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"EveningAvgSpeedVsSpeedLimit = {item.EveningAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.EveningEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"EveningEightyFifthSpeedVsSpeedLimit = {item.EveningEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.EveningPercentObserved.HasValue)
            {
                queryBuilder.Append($"EveningPercentObserved = {item.EveningPercentObserved.Value}, ");
            }

            //////WEEKEND//////
            if (item.WeekendEveningAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendEveningAverageSpeed = {item.WeekendEveningAverageSpeed.Value}, ");
            }
            if (item.WeekendEveningAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendEveningAverageEightyFifthSpeed = {item.WeekendEveningAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekendEveningViolations.HasValue)
            {
                queryBuilder.Append($"WeekendEveningViolations = {item.WeekendEveningViolations.Value}, ");
            }

            if (item.WeekendEveningExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendEveningExtremeViolations = {item.WeekendEveningExtremeViolations.Value}, ");
            }

            if (item.WeekendEveningFlow.HasValue)
            {
                queryBuilder.Append($"WeekendEveningFlow = {item.WeekendEveningFlow.Value}, ");
            }

            if (item.WeekendEveningMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendEveningMinSpeed = {item.WeekendEveningMinSpeed.Value}, ");
            }

            if (item.WeekendEveningMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendEveningMaxSpeed = {item.WeekendEveningMaxSpeed.Value}, ");
            }

            if (item.WeekendEveningVariability.HasValue)
            {
                queryBuilder.Append($"WeekendEveningVariability = {item.WeekendEveningVariability.Value}, ");
            }

            if (item.WeekendEveningPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekendEveningPercentViolations = {item.WeekendEveningPercentViolations.Value}, ");
            }

            if (item.WeekendEveningPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendEveningPercentExtremeViolations = {item.WeekendEveningPercentExtremeViolations.Value}, ");
            }

            if (item.WeekendEveningAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendEveningAvgSpeedVsSpeedLimit = {item.WeekendEveningAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekendEveningEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendEveningEightyFifthSpeedVsSpeedLimit = {item.WeekendEveningEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekendEveningPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekendEveningPercentObserved = {item.WeekendEveningPercentObserved.Value}, ");
            }

            //////WEEKDAY//////
            if (item.WeekdayEveningAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningAverageSpeed = {item.WeekdayEveningAverageSpeed.Value}, ");
            }
            if (item.WeekdayEveningAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningAverageEightyFifthSpeed = {item.WeekdayEveningAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekdayEveningViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningViolations = {item.WeekdayEveningViolations.Value}, ");
            }

            if (item.WeekdayEveningExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningExtremeViolations = {item.WeekdayEveningExtremeViolations.Value}, ");
            }

            if (item.WeekdayEveningFlow.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningFlow = {item.WeekdayEveningFlow.Value}, ");
            }

            if (item.WeekdayEveningMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningMinSpeed = {item.WeekdayEveningMinSpeed.Value}, ");
            }

            if (item.WeekdayEveningMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningMaxSpeed = {item.WeekdayEveningMaxSpeed.Value}, ");
            }

            if (item.WeekdayEveningVariability.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningVariability = {item.WeekdayEveningVariability.Value}, ");
            }

            if (item.WeekdayEveningPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningPercentViolations = {item.WeekdayEveningPercentViolations.Value}, ");
            }

            if (item.WeekdayEveningPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningPercentExtremeViolations = {item.WeekdayEveningPercentExtremeViolations.Value}, ");
            }

            if (item.WeekdayEveningAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningAvgSpeedVsSpeedLimit = {item.WeekdayEveningAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekdayEveningEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningEightyFifthSpeedVsSpeedLimit = {item.WeekdayEveningEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekdayEveningPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekdayEveningPercentObserved = {item.WeekdayEveningPercentObserved.Value}, ");
            }


            /////////////////////////////
            ///////EARLY MORNING ////////
            /////////////////////////////

            if (item.EarlyMorningAverageSpeed.HasValue)
            {
                queryBuilder.Append($"EarlyMorningAverageSpeed = {item.EarlyMorningAverageSpeed.Value}, ");
            }

            if (item.EarlyMorningAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"EarlyMorningAverageEightyFifthSpeed = {item.EarlyMorningAverageEightyFifthSpeed.Value}, ");
            }

            if (item.EarlyMorningViolations.HasValue)
            {
                queryBuilder.Append($"EarlyMorningViolations = {item.EarlyMorningViolations.Value}, ");
            }

            if (item.EarlyMorningExtremeViolations.HasValue)
            {
                queryBuilder.Append($"EarlyMorningExtremeViolations = {item.EarlyMorningExtremeViolations.Value}, ");
            }

            if (item.EarlyMorningFlow.HasValue)
            {
                queryBuilder.Append($"EarlyMorningFlow = {item.EarlyMorningFlow.Value}, ");
            }

            if (item.EarlyMorningMinSpeed.HasValue)
            {
                queryBuilder.Append($"EarlyMorningMinSpeed = {item.EarlyMorningMinSpeed.Value}, ");
            }

            if (item.EarlyMorningMaxSpeed.HasValue)
            {
                queryBuilder.Append($"EarlyMorningMaxSpeed = {item.EarlyMorningMaxSpeed.Value}, ");
            }

            if (item.EarlyMorningVariability.HasValue)
            {
                queryBuilder.Append($"EarlyMorningVariability = {item.EarlyMorningVariability.Value}, ");
            }

            if (item.EarlyMorningPercentViolations.HasValue)
            {
                queryBuilder.Append($"EarlyMorningPercentViolations = {item.EarlyMorningPercentViolations.Value}, ");
            }

            if (item.EarlyMorningPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"EarlyMorningPercentExtremeViolations = {item.EarlyMorningPercentExtremeViolations.Value}, ");
            }

            if (item.EarlyMorningAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"EarlyMorningAvgSpeedVsSpeedLimit = {item.EarlyMorningAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.EarlyMorningEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"EarlyMorningEightyFifthSpeedVsSpeedLimit = {item.EarlyMorningEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.EarlyMorningPercentObserved.HasValue)
            {
                queryBuilder.Append($"EarlyMorningPercentObserved = {item.EarlyMorningPercentObserved.Value}, ");
            }

            //////WEEKEND//////
            if (item.WeekendEarlyMorningAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningAverageSpeed = {item.WeekendEarlyMorningAverageSpeed.Value}, ");
            }
            if (item.WeekendEarlyMorningAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningAverageEightyFifthSpeed = {item.WeekendEarlyMorningAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekendEarlyMorningViolations.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningViolations = {item.WeekendEarlyMorningViolations.Value}, ");
            }

            if (item.WeekendEarlyMorningExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningExtremeViolations = {item.WeekendEarlyMorningExtremeViolations.Value}, ");
            }

            if (item.WeekendEarlyMorningFlow.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningFlow = {item.WeekendEarlyMorningFlow.Value}, ");
            }

            if (item.WeekendEarlyMorningMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningMinSpeed = {item.WeekendEarlyMorningMinSpeed.Value}, ");
            }

            if (item.WeekendEarlyMorningMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningMaxSpeed = {item.WeekendEarlyMorningMaxSpeed.Value}, ");
            }

            if (item.WeekendEarlyMorningVariability.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningVariability = {item.WeekendEarlyMorningVariability.Value}, ");
            }

            if (item.WeekendEarlyMorningPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningPercentViolations = {item.WeekendEarlyMorningPercentViolations.Value}, ");
            }

            if (item.WeekendEarlyMorningPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningPercentExtremeViolations = {item.WeekendEarlyMorningPercentExtremeViolations.Value}, ");
            }

            if (item.WeekendEarlyMorningAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningAvgSpeedVsSpeedLimit = {item.WeekendEarlyMorningAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit = {item.WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekendEarlyMorningPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekendEarlyMorningPercentObserved = {item.WeekendEarlyMorningPercentObserved.Value}, ");
            }

            //////WEEKDAY//////
            if (item.WeekdayEarlyMorningAverageSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningAverageSpeed = {item.WeekdayEarlyMorningAverageSpeed.Value}, ");
            }
            if (item.WeekdayEarlyMorningAverageEightyFifthSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningAverageEightyFifthSpeed = {item.WeekdayEarlyMorningAverageEightyFifthSpeed.Value}, ");
            }

            if (item.WeekdayEarlyMorningViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningViolations = {item.WeekdayEarlyMorningViolations.Value}, ");
            }

            if (item.WeekdayEarlyMorningExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningExtremeViolations = {item.WeekdayEarlyMorningExtremeViolations.Value}, ");
            }

            if (item.WeekdayEarlyMorningFlow.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningFlow = {item.WeekdayEarlyMorningFlow.Value}, ");
            }

            if (item.WeekdayEarlyMorningMinSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningMinSpeed = {item.WeekdayEarlyMorningMinSpeed.Value}, ");
            }

            if (item.WeekdayEarlyMorningMaxSpeed.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningMaxSpeed = {item.WeekdayEarlyMorningMaxSpeed.Value}, ");
            }

            if (item.WeekdayEarlyMorningVariability.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningVariability = {item.WeekdayEarlyMorningVariability.Value}, ");
            }

            if (item.WeekdayEarlyMorningPercentViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningPercentViolations = {item.WeekdayEarlyMorningPercentViolations.Value}, ");
            }

            if (item.WeekdayEarlyMorningPercentExtremeViolations.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningPercentExtremeViolations = {item.WeekdayEarlyMorningPercentExtremeViolations.Value}, ");
            }

            if (item.WeekdayEarlyMorningAvgSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningAvgSpeedVsSpeedLimit = {item.WeekdayEarlyMorningAvgSpeedVsSpeedLimit.Value}, ");
            }

            if (item.WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit = {item.WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit.Value}, ");
            }
            if (item.WeekdayEarlyMorningPercentObserved.HasValue)
            {
                queryBuilder.Append($"WeekdayEarlyMorningPercentObserved = {item.WeekdayEarlyMorningPercentObserved.Value}, ");
            }

            // Remove the last comma and space if present
            if (queryBuilder[queryBuilder.Length - 2] == ',')
            {
                queryBuilder.Length -= 2;
            }

            if (item.Id != null)
            {
                queryBuilder.Append($" WHERE Id = '{item.Id}'");
            }
            else
            {
                queryBuilder.Append($" WHERE BinStartTime = TIMESTAMP('{item.BinStartTime:yyyy-MM-dd HH:mm:ss}') AND SegmentId = '{item.SegmentId}' AND SourceId = {item.SourceId}");
            }

            var query = queryBuilder.ToString();
            return query;
        }

    }
}
