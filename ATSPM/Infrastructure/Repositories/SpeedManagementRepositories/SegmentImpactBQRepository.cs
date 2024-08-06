using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using Google.Cloud.BigQuery.V2;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementRepositories
{
    ///<inheritdoc cref="IImpactTypeRepository"/>
    public class SegmentImpactBQRepository : ATSPMRepositoryBQBase<SegmentImpact>, ISegmentImpactRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<SegmentImpact>> _logger;

        public SegmentImpactBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<SegmentImpact>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
        }

        public override IQueryable<SegmentImpact> GetList()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = _client.ExecuteQuery(query, parameters).ToList();

            return result.Select(row => MapRowToEntity(row)).ToList().AsQueryable();
        }

        public async Task<IReadOnlyList<SegmentImpact>> GetSegmentsForImpactAsync(Guid impactId)
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString())
            };

            var result = await _client.ExecuteQueryAsync(query, parameters);

            return result.Select(row => MapRowToEntity(row)).ToList().AsReadOnly();
        }

        public override SegmentImpact Lookup(object key)
        {
            var impactId = (long)key.GetType().GetProperty("ImpactId").GetValue(key);
            var segmentId = (long)key.GetType().GetProperty("SegmentId").GetValue(key);
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND SegmentId = @segmentId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            var results = _client.ExecuteQuery(query, parameters);
            Task<SegmentImpact> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override SegmentImpact Lookup(SegmentImpact item)
        {
            var impactId = item.ImpactId;
            var segmentId = item.SegmentId;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND SegmentId = @segmentId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            var results = _client.ExecuteQuery(query, parameters);
            Task<SegmentImpact> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override async Task<SegmentImpact> LookupAsync(object key)
        {
            var impactId = (long)key.GetType().GetProperty("ImpactId").GetValue(key);
            var segmentId = (long)key.GetType().GetProperty("SegmentId").GetValue(key);
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND SegmentId = @segmentId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override async Task<SegmentImpact> LookupAsync(SegmentImpact item)
        {
            var impactId = item.ImpactId;
            var segmentId = item.SegmentId;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND SegmentId = @segmentId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override void Remove(SegmentImpact item)
        {
            var impactId = item.ImpactId;
            var segmentId = item.SegmentId;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND SegmentId = @segmentId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override async Task RemoveAsync(SegmentImpact item)
        {
            var impactId = item.ImpactId;
            var segmentId = item.SegmentId;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND SegmentId = @segmentId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        /// <inheritdoc/>

        public async Task RemoveAllSegmentsFromImpactIdAsync(Guid? impactId)
        {
            if (impactId == null)
            {
                return;
            }
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString())
            };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        /// <inheritdoc/>

        public async Task RemoveAllImpactsFromSegmentAsync(Guid segmentId)
        {
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE SegmentId = @segmentId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("segmentId", BigQueryDbType.String, segmentId.ToString())
            };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<SegmentImpact> items)
        {
            foreach (var item in items)
            {
                Remove(item);
            }
        }

        public override async Task RemoveRangeAsync(IEnumerable<SegmentImpact> items)
        {
            foreach (var item in items)
            {
                RemoveAsync(item);
            }
        }

        public override async void Update(SegmentImpact item)
        {
            // Lookup the existing row based on the Id
            var oldRow = await LookupAsync(item);
            if (oldRow != null)
            {
                return;
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` (ImpactId, SegmentId) VALUES ('{item.ImpactId}', '{item.SegmentId}')";
                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQuery(query, parameters);
            }
        }

        public override async Task UpdateAsync(SegmentImpact item)
        {
            var oldRow = await LookupAsync(item);
            if (oldRow != null)
            {
                return;
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` (ImpactId, SegmentId) VALUES ('{item.ImpactId}', '{item.SegmentId}')";
                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
            }
        }

        public override void UpdateRange(IEnumerable<SegmentImpact> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public override async Task UpdateRangeAsync(IEnumerable<SegmentImpact> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }

        protected override BigQueryInsertRow CreateRow(SegmentImpact item)
        {
            return new BigQueryInsertRow
            {
                {"ImpactId", item.ImpactId },
                {"SegmentId", item.SegmentId }
            };
        }

        protected override SegmentImpact MapRowToEntity(BigQueryRow row)
        {
            var bigQueryImpactId = Guid.Parse(row["ImpactId"].ToString());
            var bigQuerySegmentId = Guid.Parse(row["SegmentId"].ToString());
            return new SegmentImpact
            {
                ImpactId = bigQueryImpactId,
                SegmentId = bigQuerySegmentId
            };
        }
    }
}
