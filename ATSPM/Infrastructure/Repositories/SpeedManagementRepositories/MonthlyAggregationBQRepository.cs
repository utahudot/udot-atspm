using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementRepositories
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
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                string query = updateQuery(item);

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@key", BigQueryDbType.String, item.Id.ToString())
        };

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
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                string query = updateQuery(item);

                var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("@key", BigQueryDbType.String, item.Id.ToString())
                };

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
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                string query = updateQuery(item);

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@key", BigQueryDbType.String, item.Id.ToString())
        };

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

        protected override BigQueryInsertRow CreateRow(MonthlyAggregation item)
        {
            return new BigQueryInsertRow
    {
        { "Id", item.Id },
        { "CreatedDate", item.CreatedDate },
        { "BinStartTime", item.BinStartTime },
        { "SegmentId", item.SegmentId },
        { "SourceId", item.SourceId },
        { "AllDayAverageSpeed", item.AllDayAverageSpeed },
        { "AllDayViolations", item.AllDayViolations },
        { "AllDayExtremeViolations", item.AllDayExtremeViolations },
        { "OffPeakAverageSpeed", item.OffPeakAverageSpeed },
        { "OffPeakViolations", item.OffPeakViolations },
        { "OffPeakExtremeViolations", item.OffPeakExtremeViolations },
        { "AmPeakAverageSpeed", item.AmPeakAverageSpeed },
        { "AmPeakViolations", item.AmPeakViolations },
        { "AmPeakExtremeViolations", item.AmPeakExtremeViolations },
        { "PmPeakAverageSpeed", item.PmPeakAverageSpeed },
        { "PmPeakViolations", item.PmPeakViolations },
        { "PmPeakExtremeViolations", item.PmPeakExtremeViolations },
        { "MidDayAverageSpeed", item.MidDayAverageSpeed },
        { "MidDayViolations", item.MidDayViolations },
        { "MidDayExtremeViolations", item.MidDayExtremeViolations },
        { "EveningAverageSpeed", item.EveningAverageSpeed },
        { "EveningViolations", item.EveningViolations },
        { "EveningExtremeViolations", item.EveningExtremeViolations },
        { "EarlyMorningAverageSpeed", item.EarlyMorningAverageSpeed },
        { "EarlyMorningViolations", item.EarlyMorningViolations },
        { "EarlyMorningExtremeViolations", item.EarlyMorningExtremeViolations },
        { "DataQuality", item.DataQuality }
    };
        }

        protected override MonthlyAggregation MapRowToEntity(BigQueryRow row)
        {
            var bigQueryId = Guid.Parse(row["Id"].ToString());
            var bigQueryCreatedDate = DateTime.Parse(row["CreatedDate"].ToString());
            var bigQueryBinStartTime = DateTime.Parse(row["BinStartTime"].ToString());
            var bigQuerySegmentId = Guid.Parse(row["SegmentId"].ToString());
            var bigQuerySourceId = int.Parse(row["SourceId"].ToString());
            var bigQueryAllDayAverageSpeed = row["AllDayAverageSpeed"] != null ? int.Parse(row["AllDayAverageSpeed"].ToString()) : (int?)null;
            var bigQueryAllDayViolations = row["AllDayViolations"] != null ? int.Parse(row["AllDayViolations"].ToString()) : (int?)null;
            var bigQueryAllDayExtremeViolations = row["AllDayExtremeViolations"] != null ? int.Parse(row["AllDayExtremeViolations"].ToString()) : (int?)null;
            var bigQueryOffPeakAverageSpeed = row["OffPeakAverageSpeed"] != null ? int.Parse(row["OffPeakAverageSpeed"].ToString()) : (int?)null;
            var bigQueryOffPeakViolations = row["OffPeakViolations"] != null ? int.Parse(row["OffPeakViolations"].ToString()) : (int?)null;
            var bigQueryOffPeakExtremeViolations = row["OffPeakExtremeViolations"] != null ? int.Parse(row["OffPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryAmPeakAverageSpeed = row["AmPeakAverageSpeed"] != null ? int.Parse(row["AmPeakAverageSpeed"].ToString()) : (int?)null;
            var bigQueryAmPeakViolations = row["AmPeakViolations"] != null ? int.Parse(row["AmPeakViolations"].ToString()) : (int?)null;
            var bigQueryAmPeakExtremeViolations = row["AmPeakExtremeViolations"] != null ? int.Parse(row["AmPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryPmPeakAverageSpeed = row["PmPeakAverageSpeed"] != null ? int.Parse(row["PmPeakAverageSpeed"].ToString()) : (int?)null;
            var bigQueryPmPeakViolations = row["PmPeakViolations"] != null ? int.Parse(row["PmPeakViolations"].ToString()) : (int?)null;
            var bigQueryPmPeakExtremeViolations = row["PmPeakExtremeViolations"] != null ? int.Parse(row["PmPeakExtremeViolations"].ToString()) : (int?)null;
            var bigQueryMidDayAverageSpeed = row["MidDayAverageSpeed"] != null ? int.Parse(row["MidDayAverageSpeed"].ToString()) : (int?)null;
            var bigQueryMidDayViolations = row["MidDayViolations"] != null ? int.Parse(row["MidDayViolations"].ToString()) : (int?)null;
            var bigQueryMidDayExtremeViolations = row["MidDayExtremeViolations"] != null ? int.Parse(row["MidDayExtremeViolations"].ToString()) : (int?)null;
            var bigQueryEveningAverageSpeed = row["EveningAverageSpeed"] != null ? int.Parse(row["EveningAverageSpeed"].ToString()) : (int?)null;
            var bigQueryEveningViolations = row["EveningViolations"] != null ? int.Parse(row["EveningViolations"].ToString()) : (int?)null;
            var bigQueryEveningExtremeViolations = row["EveningExtremeViolations"] != null ? int.Parse(row["EveningExtremeViolations"].ToString()) : (int?)null;
            var bigQueryEarlyMorningAverageSpeed = row["EarlyMorningAverageSpeed"] != null ? int.Parse(row["EarlyMorningAverageSpeed"].ToString()) : (int?)null;
            var bigQueryEarlyMorningViolations = row["EarlyMorningViolations"] != null ? int.Parse(row["EarlyMorningViolations"].ToString()) : (int?)null;
            var bigQueryEarlyMorningExtremeViolations = row["EarlyMorningExtremeViolations"] != null ? int.Parse(row["EarlyMorningExtremeViolations"].ToString()) : (int?)null;
            var bigQueryDataQuality = bool.Parse(row["DataQuality"].ToString());

            return new MonthlyAggregation
            {
                Id = bigQueryId,
                CreatedDate = bigQueryCreatedDate,
                BinStartTime = bigQueryBinStartTime,
                SegmentId = bigQuerySegmentId,
                SourceId = bigQuerySourceId,
                AllDayAverageSpeed = bigQueryAllDayAverageSpeed,
                AllDayViolations = bigQueryAllDayViolations,
                AllDayExtremeViolations = bigQueryAllDayExtremeViolations,
                OffPeakAverageSpeed = bigQueryOffPeakAverageSpeed,
                OffPeakViolations = bigQueryOffPeakViolations,
                OffPeakExtremeViolations = bigQueryOffPeakExtremeViolations,
                AmPeakAverageSpeed = bigQueryAmPeakAverageSpeed,
                AmPeakViolations = bigQueryAmPeakViolations,
                AmPeakExtremeViolations = bigQueryAmPeakExtremeViolations,
                PmPeakAverageSpeed = bigQueryPmPeakAverageSpeed,
                PmPeakViolations = bigQueryPmPeakViolations,
                PmPeakExtremeViolations = bigQueryPmPeakExtremeViolations,
                MidDayAverageSpeed = bigQueryMidDayAverageSpeed,
                MidDayViolations = bigQueryMidDayViolations,
                MidDayExtremeViolations = bigQueryMidDayExtremeViolations,
                EveningAverageSpeed = bigQueryEveningAverageSpeed,
                EveningViolations = bigQueryEveningViolations,
                EveningExtremeViolations = bigQueryEveningExtremeViolations,
                EarlyMorningAverageSpeed = bigQueryEarlyMorningAverageSpeed,
                EarlyMorningViolations = bigQueryEarlyMorningViolations,
                EarlyMorningExtremeViolations = bigQueryEarlyMorningExtremeViolations,
                DataQuality = bigQueryDataQuality
            };
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
            var thresholdDate = DateTime.UtcNow.AddYears(-2);

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

        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

        private string InsertQueryStatement(MonthlyAggregation item)
        {
            return $"INSERT INTO `{_datasetId}.{_tableId}` " +
                $"(Id, CreatedDate, BinStartTime, SegmentId, SourceId, AllDayAverageSpeed, AllDayViolations, AllDayExtremeViolations, OffPeakAverageSpeed, OffPeakViolations, OffPeakExtremeViolations, AmPeakAverageSpeed, AmPeakViolations, AmPeakExtremeViolations, PmPeakAverageSpeed, PmPeakViolations, PmPeakExtremeViolations, MidDayAverageSpeed, MidDayViolations, MidDayExtremeViolations, EveningAverageSpeed, EveningViolations, EveningExtremeViolations, EarlyMorningAverageSpeed, EarlyMorningViolations, EarlyMorningExtremeViolations, DataQuality) " +
                $"VALUES (" +
                $"GENERATE_UUID(), " +
                $"CURRENT_TIMESTAMP(), " +
                $"TIMESTAMP('{item.BinStartTime:yyyy-MM-dd HH:mm:ss}'), " +
                $"'{item.SegmentId}', " +
                $"{item.SourceId}, " +
                $"{(item.AllDayAverageSpeed.HasValue ? item.AllDayAverageSpeed.Value.ToString() : "NULL")}, " +
                $"{(item.AllDayViolations.HasValue ? item.AllDayViolations.Value.ToString() : "NULL")}, " +
                $"{(item.AllDayExtremeViolations.HasValue ? item.AllDayExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.OffPeakAverageSpeed.HasValue ? item.OffPeakAverageSpeed.Value.ToString() : "NULL")}, " +
                $"{(item.OffPeakViolations.HasValue ? item.OffPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.OffPeakExtremeViolations.HasValue ? item.OffPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.AmPeakAverageSpeed.HasValue ? item.AmPeakAverageSpeed.Value.ToString() : "NULL")}, " +
                $"{(item.AmPeakViolations.HasValue ? item.AmPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.AmPeakExtremeViolations.HasValue ? item.AmPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.PmPeakAverageSpeed.HasValue ? item.PmPeakAverageSpeed.Value.ToString() : "NULL")}, " +
                $"{(item.PmPeakViolations.HasValue ? item.PmPeakViolations.Value.ToString() : "NULL")}, " +
                $"{(item.PmPeakExtremeViolations.HasValue ? item.PmPeakExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.MidDayAverageSpeed.HasValue ? item.MidDayAverageSpeed.Value.ToString() : "NULL")}, " +
                $"{(item.MidDayViolations.HasValue ? item.MidDayViolations.Value.ToString() : "NULL")}, " +
                $"{(item.MidDayExtremeViolations.HasValue ? item.MidDayExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.EveningAverageSpeed.HasValue ? item.EveningAverageSpeed.Value.ToString() : "NULL")}, " +
                $"{(item.EveningViolations.HasValue ? item.EveningViolations.Value.ToString() : "NULL")}, " +
                $"{(item.EveningExtremeViolations.HasValue ? item.EveningExtremeViolations.Value.ToString() : "NULL")}, " +
                $"{(item.EarlyMorningAverageSpeed.HasValue ? item.EarlyMorningAverageSpeed.Value.ToString() : "NULL")}, " +
                $"{(item.EarlyMorningViolations.HasValue ? item.EarlyMorningViolations.Value.ToString() : "NULL")}, " +
                $"{(item.EarlyMorningExtremeViolations.HasValue ? item.EarlyMorningExtremeViolations.Value.ToString() : "NULL")}, " +
            $"{item.DataQuality})";
        }

        private string updateQuery(MonthlyAggregation item)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");
            queryBuilder.Append($"BinStartTime = '{item.BinStartTime:O}', ");
            queryBuilder.Append($"SegmentId = '{item.SegmentId}', ");
            queryBuilder.Append($"SourceId = {item.SourceId}, ");

            if (item.AllDayAverageSpeed.HasValue)
            {
                queryBuilder.Append($"AllDayAverageSpeed = {item.AllDayAverageSpeed.Value}, ");
            }

            if (item.AllDayViolations.HasValue)
            {
                queryBuilder.Append($"AllDayViolations = {item.AllDayViolations.Value}, ");
            }

            if (item.AllDayExtremeViolations.HasValue)
            {
                queryBuilder.Append($"AllDayExtremeViolations = {item.AllDayExtremeViolations.Value}, ");
            }

            if (item.OffPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"OffPeakAverageSpeed = {item.OffPeakAverageSpeed.Value}, ");
            }

            if (item.OffPeakViolations.HasValue)
            {
                queryBuilder.Append($"OffPeakViolations = {item.OffPeakViolations.Value}, ");
            }

            if (item.OffPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"OffPeakExtremeViolations = {item.OffPeakExtremeViolations.Value}, ");
            }

            if (item.AmPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"AmPeakAverageSpeed = {item.AmPeakAverageSpeed.Value}, ");
            }

            if (item.AmPeakViolations.HasValue)
            {
                queryBuilder.Append($"AmPeakViolations = {item.AmPeakViolations.Value}, ");
            }

            if (item.AmPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"AmPeakExtremeViolations = {item.AmPeakExtremeViolations.Value}, ");
            }

            if (item.PmPeakAverageSpeed.HasValue)
            {
                queryBuilder.Append($"PmPeakAverageSpeed = {item.PmPeakAverageSpeed.Value}, ");
            }

            if (item.PmPeakViolations.HasValue)
            {
                queryBuilder.Append($"PmPeakViolations = {item.PmPeakViolations.Value}, ");
            }

            if (item.PmPeakExtremeViolations.HasValue)
            {
                queryBuilder.Append($"PmPeakExtremeViolations = {item.PmPeakExtremeViolations.Value}, ");
            }

            if (item.MidDayAverageSpeed.HasValue)
            {
                queryBuilder.Append($"MidDayAverageSpeed = {item.MidDayAverageSpeed.Value}, ");
            }

            if (item.MidDayViolations.HasValue)
            {
                queryBuilder.Append($"MidDayViolations = {item.MidDayViolations.Value}, ");
            }

            if (item.MidDayExtremeViolations.HasValue)
            {
                queryBuilder.Append($"MidDayExtremeViolations = {item.MidDayExtremeViolations.Value}, ");
            }

            if (item.EveningAverageSpeed.HasValue)
            {
                queryBuilder.Append($"EveningAverageSpeed = {item.EveningAverageSpeed.Value}, ");
            }

            if (item.EveningViolations.HasValue)
            {
                queryBuilder.Append($"EveningViolations = {item.EveningViolations.Value}, ");
            }

            if (item.EveningExtremeViolations.HasValue)
            {
                queryBuilder.Append($"EveningExtremeViolations = {item.EveningExtremeViolations.Value}, ");
            }

            if (item.EarlyMorningAverageSpeed.HasValue)
            {
                queryBuilder.Append($"EarlyMorningAverageSpeed = {item.EarlyMorningAverageSpeed.Value}, ");
            }

            if (item.EarlyMorningViolations.HasValue)
            {
                queryBuilder.Append($"EarlyMorningViolations = {item.EarlyMorningViolations.Value}, ");
            }

            if (item.EarlyMorningExtremeViolations.HasValue)
            {
                queryBuilder.Append($"EarlyMorningExtremeViolations = {item.EarlyMorningExtremeViolations.Value}, ");
            }

            queryBuilder.Append($"DataQuality = {item.DataQuality}");

            // Remove the last comma and space if present
            if (queryBuilder[queryBuilder.Length - 2] == ',')
            {
                queryBuilder.Length -= 2;
            }

            queryBuilder.Append($" WHERE Id = @key");

            var query = queryBuilder.ToString();
            return query;
        }



    }
}
