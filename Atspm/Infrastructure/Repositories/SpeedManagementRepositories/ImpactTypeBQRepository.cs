﻿using Google.Cloud.BigQuery.V2;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories
{
    ///<inheritdoc cref="IImpactTypeRepository"/>
    public class ImpactTypeBQRepository : ATSPMRepositoryBQBase<ImpactType>, IImpactTypeRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<ImpactType>> _logger;

        public ImpactTypeBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<ImpactType>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
        }

        public override IQueryable<ImpactType> GetList()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = _client.ExecuteQuery(query, parameters).ToList();

            // Map the result to a list of ImpactType objects
            return result.Select(row => MapRowToEntity(row)).ToList().AsQueryable();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ImpactType>> GetListImpactTypeAsync()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = await _client.ExecuteQueryAsync(query, parameters);

            // Fetch the rows asynchronously
            var rows = result.ToList();

            // Map the result to a list of ImpactType objects using LINQ
            var impactTypes = rows.Select(row => MapRowToEntity(row)).ToList().AsReadOnly();

            return impactTypes;
        }

        public override ImpactType Lookup(object key)
        {
            if (key == null)
            {
                return null;
            }
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key)
                };
            var results = _client.ExecuteQuery(query, parameters);
            Task<ImpactType> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override ImpactType Lookup(ImpactType item)
        {
            if (item.Id == null)
            {
                return null;
            }
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item.Id)
                };

            var results = _client.ExecuteQuery(query, parameters);
            Task<ImpactType> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override async Task<ImpactType> LookupAsync(object key)
        {
            if (key == null)
            {
                return null;
            }
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key.ToString())
                };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override async Task<ImpactType> LookupAsync(ImpactType item)
        {
            if (item.Id == null)
            {
                return null;
            }
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
             };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override void Remove(ImpactType item)
        {
            if (item.Id == null)
            {
                return;
            }
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
             };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override async Task RemoveAsync(ImpactType item)
        {
            if (item.Id == null)
            {
                return;
            }
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
             };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<ImpactType> items)
        {
            var ids = string.Join(", ", items.Select(i => i.Id));
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>();

            _client.ExecuteQuery(query, parameters);
        }

        public override async Task RemoveRangeAsync(IEnumerable<ImpactType> items)
        {
            var ids = string.Join(", ", items.Select(i => i.Id));
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>
            {
                 new BigQueryParameter("ids", BigQueryDbType.String, ids)
             };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override async void Update(ImpactType item)
        {
            // Lookup the existing row based on the Id
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                var (query, parameters) = UpdateQuery(item);

                _client.ExecuteQuery(query, parameters);
            }
            else
            {
                var (query, id) = InsertQuery(item);
                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQuery(query, parameters);
            }
        }

        public override async Task<ImpactType> UpdateAsync(ImpactType item)
        {
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                var (query, parameters) = UpdateQuery(item);

                await _client.ExecuteQueryAsync(query, parameters);
                return await LookupAsync(item.Id);
            }
            else
            {
                var (query, id) = InsertQuery(item);
                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
                return await LookupAsync(id);
            }
        }

        public async Task<ImpactType> UpsertImpactType(ImpactType item)
        {
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                var (query, parameters) = UpdateQuery(item);

                await _client.ExecuteQueryAsync(query, parameters);
                return await LookupAsync(item.Id);
            }
            else
            {
                var (query, id) = InsertQuery(item);
                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
                return await LookupAsync(id);
            }
        }

        public override void UpdateRange(IEnumerable<ImpactType> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public override async Task UpdateRangeAsync(IEnumerable<ImpactType> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }

        protected override BigQueryInsertRow CreateRow(ImpactType item)
        {
            return new BigQueryInsertRow
            {
                {"Id", item.Id },
                {"Name", item.Name },
                {"Description", item.Description }
            };
        }

        protected override ImpactType MapRowToEntity(BigQueryRow row)
        {
            var bigQueryName = row["Name"].ToString();
            var bigQueryDescription = row["Description"].ToString();
            var bigQueryId = Guid.Parse(row["Id"].ToString());

            return new ImpactType
            {
                Id = bigQueryId,
                Name = bigQueryName,
                Description = bigQueryDescription
            };
        }

        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

        private Tuple<string, List<BigQueryParameter>> UpdateQuery(ImpactType item)
        {
            // Create a new row with updated values
            var query = $"UPDATE `{_datasetId}.{_tableId}` SET Name = '{item.Name}', Description = '{item.Description}' WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
             };
            return new Tuple<string, List<BigQueryParameter>>(query, parameters);
        }

        private Tuple<string, Guid> InsertQuery(ImpactType item)
        {
            Guid id = Guid.NewGuid();
            var query = $"INSERT INTO `{_datasetId}.{_tableId}` (Id, Name, Description) VALUES ('{id}', '{item.Name}', '{item.Description}')";
            return new Tuple<string, Guid>(query, id);
        }
    }
}