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

                PercentObserved = bigQueryPercentObserved
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

        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

        private string InsertQueryStatement(MonthlyAggregation item)
        {
            return $"INSERT INTO `{_datasetId}.{_tableId}` " +
                $"(Id, CreatedDate, BinStartTime, SegmentId, SourceId, " +
                $"AllDayAverageSpeed, AllDayAverageEightyFifthSpeed, AllDayViolations, AllDayExtremeViolations, AllDayFlow, AllDayMinSpeed, AllDayMaxSpeed, AllDayVariability, AllDayPercentViolations, AllDayPercentExtremeViolations, AllDayAvgSpeedVsSpeedLimit, AllDayEightyFifthSpeedVsSpeedLimit, " +
                $"OffPeakAverageSpeed, OffPeakAverageEightyFifthSpeed, OffPeakViolations, OffPeakExtremeViolations, OffPeakFlow, OffPeakMinSpeed, OffPeakMaxSpeed, OffPeakVariability, OffPeakPercentViolations, OffPeakPercentExtremeViolations, OffPeakAvgSpeedVsSpeedLimit, OffPeakEightyFifthSpeedVsSpeedLimit, " +
                $"AmPeakAverageSpeed, AmPeakAverageEightyFifthSpeed, AmPeakViolations, AmPeakExtremeViolations, AmPeakFlow, AmPeakMinSpeed, AmPeakMaxSpeed, AmPeakVariability, AmPeakPercentViolations, AmPeakPercentExtremeViolations, AmPeakAvgSpeedVsSpeedLimit, AmPeakEightyFifthSpeedVsSpeedLimit, " +
                $"PmPeakAverageSpeed, PmPeakAverageEightyFifthSpeed, PmPeakViolations, PmPeakExtremeViolations, PmPeakFlow, PmPeakMinSpeed, PmPeakMaxSpeed, PmPeakVariability, PmPeakPercentViolations, PmPeakPercentExtremeViolations, PmPeakAvgSpeedVsSpeedLimit, PmPeakEightyFifthSpeedVsSpeedLimit, " +
                $"MidDayAverageSpeed, MidDayAverageEightyFifthSpeed, MidDayViolations, MidDayExtremeViolations, MidDayFlow, MidDayMinSpeed, MidDayMaxSpeed, MidDayVariability, MidDayPercentViolations, MidDayPercentExtremeViolations, MidDayAvgSpeedVsSpeedLimit, MidDayEightyFifthSpeedVsSpeedLimit, " +
                $"EveningAverageSpeed, EveningAverageEightyFifthSpeed, EveningViolations, EveningExtremeViolations, EveningFlow, EveningMinSpeed, EveningMaxSpeed, EveningVariability, EveningPercentViolations, EveningPercentExtremeViolations, EveningAvgSpeedVsSpeedLimit, EveningEightyFifthSpeedVsSpeedLimit, " +
                $"EarlyMorningAverageSpeed, EarlyMorningAverageEightyFifthSpeed, EarlyMorningViolations, EarlyMorningExtremeViolations, EarlyMorningFlow, EarlyMorningMinSpeed, EarlyMorningMaxSpeed, EarlyMorningVariability, EarlyMorningPercentViolations, EarlyMorningPercentExtremeViolations, EarlyMorningAvgSpeedVsSpeedLimit, EarlyMorningEightyFifthSpeedVsSpeedLimit, " +
                $"PercentObserved) " +
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

            $"{item.PercentObserved})";
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

            ////////////////////////////////////
            ///////// PERCENT OBSERVED /////////
            ////////////////////////////////////

            queryBuilder.Append($"PercentObserved = {item.PercentObserved}");

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
