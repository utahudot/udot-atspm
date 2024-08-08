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
    public class ImpactImpactTypeBQRepository : ATSPMRepositoryBQBase<ImpactImpactType>, IImpactImpactTypeRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<ImpactImpactType>> _logger;

        public ImpactImpactTypeBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<ImpactImpactType>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
        }

        public override IQueryable<ImpactImpactType> GetList()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = _client.ExecuteQuery(query, parameters).ToList();

            return result.Select(row => MapRowToEntity(row)).ToList().AsQueryable();
        }

        public override ImpactImpactType Lookup(object key)
        {
            var impactId = (long)key.GetType().GetProperty("ImpactId").GetValue(key);
            var impactTypeId = (long)key.GetType().GetProperty("ImpactTypeId").GetValue(key);
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND ImpactTypeId = @impactTypeId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("impactTypeId", BigQueryDbType.String, impactTypeId.ToString())
            };

            var results = _client.ExecuteQuery(query, parameters);
            Task<ImpactImpactType> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override ImpactImpactType Lookup(ImpactImpactType item)
        {
            var impactId = item.ImpactId;
            var impactTypeId = item.ImpactTypeId;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND ImpactTypeId = @impactTypeId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("impactTypeId", BigQueryDbType.String, impactTypeId.ToString())
            };

            var results = _client.ExecuteQuery(query, parameters);
            Task<ImpactImpactType> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override async Task<ImpactImpactType> LookupAsync(object key)
        {
            var impactId = (long)key.GetType().GetProperty("ImpactId").GetValue(key);
            var impactTypeId = (long)key.GetType().GetProperty("ImpactTypeId").GetValue(key);
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND ImpactTypeId = @impactTypeId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("impactTypeId", BigQueryDbType.String, impactTypeId.ToString())
            };

            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override async Task<ImpactImpactType> LookupAsync(ImpactImpactType item)
        {
            var impactId = item.ImpactId;
            var impactTypeId = item.ImpactTypeId;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND ImpactTypeId = @impactTypeId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("impactTypeId", BigQueryDbType.String, impactTypeId.ToString())
            };

            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override void Remove(ImpactImpactType item)
        {
            var impactId = item.ImpactId;
            var impactTypeId = item.ImpactTypeId;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND ImpactTypeId = @impactTypeId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("impactTypeId", BigQueryDbType.String, impactTypeId.ToString())
            };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override async Task RemoveAsync(ImpactImpactType item)
        {
            var impactId = item.ImpactId;
            var impactTypeId = item.ImpactTypeId;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId AND ImpactTypeId = @impactTypeId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString()),
                new BigQueryParameter("impactTypeId", BigQueryDbType.String, impactTypeId.ToString())
            };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<ImpactImpactType> items)
        {
            foreach (var item in items)
            {
                Remove(item);
            }
        }

        public override async Task RemoveRangeAsync(IEnumerable<ImpactImpactType> items)
        {
            foreach (var item in items)
            {
                RemoveAsync(item);
            }
        }

        public override async void Update(ImpactImpactType item)
        {
            // Lookup the existing row based on the Id
            var oldRow = await LookupAsync(item);
            if (oldRow != null)
            {
                return;
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` (ImpactId, ImpactTypeId) VALUES ('{item.ImpactId}', '{item.ImpactTypeId}')";
                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQuery(query, parameters);
            }
        }

        public override async Task UpdateAsync(ImpactImpactType item)
        {
            var oldRow = await LookupAsync(item);
            if (oldRow != null)
            {
                return;
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` (ImpactId, ImpactTypeId) VALUES ('{item.ImpactId}', '{item.ImpactTypeId}')";
                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
            }
        }

        public override void UpdateRange(IEnumerable<ImpactImpactType> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public override async Task UpdateRangeAsync(IEnumerable<ImpactImpactType> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }

        protected override BigQueryInsertRow CreateRow(ImpactImpactType item)
        {
            return new BigQueryInsertRow
            {
                {"ImpactId", item.ImpactId },
                {"ImpactTypeId", item.ImpactTypeId }
            };
        }

        protected override ImpactImpactType MapRowToEntity(BigQueryRow row)
        {
            var bigQueryImpactId = Guid.Parse(row["ImpactId"].ToString());
            var bigQueryImpactTypeId = Guid.Parse(row["ImpactTypeId"].ToString());
            return new ImpactImpactType
            {
                ImpactId = bigQueryImpactId,
                ImpactTypeId = bigQueryImpactTypeId
            };
        }

        public async Task<IReadOnlyList<ImpactImpactType>> GetImpactTypesForImpactAsync(Guid impactId)
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString())
            };

            var result = await _client.ExecuteQueryAsync(query, parameters);

            return result.Select(row => MapRowToEntity(row)).ToList().AsReadOnly();
        }

        public async Task RemoveAllImpactsFromImpactTypeAsync(Guid impactType)
        {
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE ImpactTypeId = @impactTypeId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactTypeId", BigQueryDbType.String, impactType.ToString())
            };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public async Task RemoveAllImpactTypesFromImpactIdAsync(Guid? impactId)
        {
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE ImpactId = @impactId";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("impactId", BigQueryDbType.String, impactId.ToString())
            };

            await _client.ExecuteQueryAsync(query, parameters);
        }
    }
}
