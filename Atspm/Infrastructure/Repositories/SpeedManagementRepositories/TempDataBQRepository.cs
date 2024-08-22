using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementRepositories
{
    public class TempDataBQRepository : ATSPMRepositoryBQBase<TempData>, ITempDataRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<TempData>> _logger;
        private BigQueryTable _table;

        public TempDataBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<TempData>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
            _table = _client.GetTable(_datasetId, _tableId);
        }

        public async Task<List<TempData>> GetHourlyAggregatedDataForAllSegments()
        {
            var query = $@"
            SELECT
              TIMESTAMP_TRUNC(BinStartTime, HOUR) AS BinStartTime,
              AVG(Average) AS Average,
              EntityId
            FROM `{_datasetId}.{_tableId}` 
            GROUP BY
            BinStartTime,
            EntityId";
            var result = await _client.ExecuteQueryAsync(query, null);
            return await MapRowToData(result);
        }

        private async Task<List<TempData>> MapRowToData(BigQueryResults result)
        {
            var tempData = new ConcurrentBag<TempData>();
            var settings = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount // or set to a specific value
            };

            var transformBlock = new TransformManyBlock<BigQueryResults, TempData>(ParseResult, settings);
            var actionBlock = new ActionBlock<TempData>(tempData.Add, settings);

            DataflowLinkOptions linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };
            transformBlock.LinkTo(actionBlock, linkOptions);

            await transformBlock.SendAsync(result);
            transformBlock.Complete();

            await actionBlock.Completion;

            return tempData.ToList();
        }

        private IEnumerable<TempData> ParseResult(BigQueryResults result)
        {
            foreach (var row in result)
            {
                yield return MapRowToEntity(row);
            }
        }

        public override IQueryable<TempData> GetList()
        {
            throw new System.NotImplementedException();
        }

        public override TempData Lookup(object key)
        {
            throw new System.NotImplementedException();
        }

        public override TempData Lookup(TempData item)
        {
            throw new System.NotImplementedException();
        }

        public override Task<TempData> LookupAsync(object key)
        {
            throw new System.NotImplementedException();
        }

        public override Task<TempData> LookupAsync(TempData item)
        {
            throw new System.NotImplementedException();
        }

        public override void Remove(TempData item)
        {
            throw new System.NotImplementedException();
        }

        public override Task RemoveAsync(TempData item)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveRange(IEnumerable<TempData> items)
        {
            throw new System.NotImplementedException();
        }

        public override Task RemoveRangeAsync(IEnumerable<TempData> items)
        {
            throw new System.NotImplementedException();
        }

        public override void Update(TempData item)
        {
            throw new System.NotImplementedException();
        }

        public override Task UpdateAsync(TempData item)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateRange(IEnumerable<TempData> items)
        {
            throw new System.NotImplementedException();
        }

        public override Task UpdateRangeAsync(IEnumerable<TempData> items)
        {
            throw new System.NotImplementedException();
        }

        protected override BigQueryInsertRow CreateRow(TempData item)
        {
            return new BigQueryInsertRow
            {
                {"BinStartTime", item.BinStartTime },
                {"Average", item.Average },
                {"EntityId", item.EntityId }
            };
        }

        protected override TempData MapRowToEntity(BigQueryRow row)
        {
            string[] formats = { "MM/dd/yyyy HH:mm:ss", "M/d/yyyy h:mm:ss tt", "yyyy-MM-ddTHH:mm:ss.fffZ" };
            var binStartTime = DateTime.ParseExact(row["BinStartTime"].ToString(), formats, null);
            var average = Double.Parse(row["Average"].ToString());
            var entityId = long.Parse(row["EntityId"].ToString());

            return new TempData
            {
                BinStartTime = binStartTime,
                Average = average,
                EntityId = entityId
            };
        }
    }
}
