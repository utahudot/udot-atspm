using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Domain.Extensions;
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

        public override async Task RemoveAsync(HourlySpeed key)
        {

            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key"; // Assuming primary key column is named "Id"
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, key) // Adjust BigQueryDbType as needed
             };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        //    public async Task UpdateAsync(T item)
        //    {
        //        var key = GetPrimaryKey(item);
        //        var updateRow = CreateRow(item);
        //        var query = $"UPDATE `{_datasetId}.{_tableId}` SET {GenerateUpdateQuery(updateRow)} WHERE Id = @key";
        //        var parameters = new List<BigQueryParameter>
        //{
        //    new BigQueryParameter("key", BigQueryDbType.String, key) // Adjust BigQueryDbType as needed
        //};
        //        await _client.ExecuteQueryAsync(query, parameters);
        //    }

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