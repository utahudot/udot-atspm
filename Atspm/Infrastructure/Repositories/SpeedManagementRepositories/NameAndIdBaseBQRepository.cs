using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories
{
    public abstract class NameAndIdBaseBQRepository : ATSPMRepositoryBQBase<NameAndIdDto>
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<NameAndIdDto>> _logger;

        protected NameAndIdBaseBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<NameAndIdDto>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
        }

        public override IQueryable<NameAndIdDto> GetList()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = _client.ExecuteQuery(query, parameters).ToList();

            return result.Select(MapRowToEntity).ToList().AsQueryable();
        }

        public override NameAndIdDto Lookup(NameAndIdDto item)
        {
            var name = item.Name;
            var id = item.Id;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Name = @name AND Id = @id";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("name", BigQueryDbType.String, name.ToString()),
                new BigQueryParameter("id", BigQueryDbType.String, id.ToString())
            };

            var results = _client.ExecuteQuery(query, parameters);
            Task<NameAndIdDto> task = Task.FromResult(results.Select(MapRowToEntity).FirstOrDefault());
            return task.Result;
        }

        public override NameAndIdDto Lookup(Object item)
        {
            var name = item.GetType().GetProperty("ImpactId").GetValue(item).ToString();
            var id = (long)item.GetType().GetProperty("SegmentId").GetValue(item);
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Name = @name AND Id = @id";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("name", BigQueryDbType.String, name.ToString()),
                new BigQueryParameter("id", BigQueryDbType.String, id.ToString())
            };

            var results = _client.ExecuteQuery(query, parameters);
            Task<NameAndIdDto> task = Task.FromResult(results.Select(MapRowToEntity).FirstOrDefault());
            return task.Result;
        }

        public override async Task<NameAndIdDto> LookupAsync(object key)
        {
            var name = key.GetType().GetProperty("Name").GetValue(key).ToString();
            var id = (long)key.GetType().GetProperty("Id").GetValue(key);
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Name = @name AND Id = @id";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("name", BigQueryDbType.String, name.ToString()),
                new BigQueryParameter("id", BigQueryDbType.String, id.ToString())
            };

            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(MapRowToEntity).FirstOrDefault();
        }

        public override async Task<NameAndIdDto> LookupAsync(NameAndIdDto item)
        {
            var name = item.Name;
            var id = item.Id;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Name = @name AND Id = @id";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("name", BigQueryDbType.String, name.ToString()),
                new BigQueryParameter("id", BigQueryDbType.String, id.ToString())
            };

            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(MapRowToEntity).FirstOrDefault();
        }

        public override void Remove(NameAndIdDto item)
        {
            var name = item.Name;
            var id = item.Id;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}`  WHERE Name = @name AND Id = @id";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("name", BigQueryDbType.String, name.ToString()),
                new BigQueryParameter("id", BigQueryDbType.String, id.ToString())
            };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override async Task RemoveAsync(NameAndIdDto item)
        {
            var name = item.Name;
            var id = item.Id;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}`  WHERE Name = @name AND Id = @id";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("name", BigQueryDbType.String, name.ToString()),
                new BigQueryParameter("id", BigQueryDbType.String, id.ToString())
            };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<NameAndIdDto> items)
        {
            foreach (var item in items)
            {
                Remove(item);
            }
        }

        public override async Task RemoveRangeAsync(IEnumerable<NameAndIdDto> items)
        {
            foreach (var item in items)
            {
                RemoveAsync(item);
            }
        }

        public override async void Update(NameAndIdDto item)
        {
            // Lookup the existing row based on the Id
            var oldRow = await LookupAsync(item);
            if (oldRow != null)
            {
                return;
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` (Name, Id) VALUES ('{item.Name}', '{item.Id}')";
                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQuery(query, parameters);
            }
        }

        public override async Task UpdateAsync(NameAndIdDto item)
        {
            var oldRow = await LookupAsync(item);
            if (oldRow != null)
            {
                return;
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` (Name, Id) VALUES ('{item.Name}', '{item.Id}')";
                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
            }
        }

        public override void UpdateRange(IEnumerable<NameAndIdDto> items)
        {
            foreach (var item in items)
            {
                UpdateAsync(item);
            }
        }

        public override async Task UpdateRangeAsync(IEnumerable<NameAndIdDto> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }

        protected override BigQueryInsertRow CreateRow(NameAndIdDto item)
        {
            return new BigQueryInsertRow
            {
                {"Name", item.Name },
                {"Id", item.Id }
            };
        }

        protected override NameAndIdDto MapRowToEntity(BigQueryRow row)
        {
            var bigQueryName = row["Name"].ToString();
            var bigQueryId = Guid.Parse(row["Id"].ToString());
            return new NameAndIdDto
            {
                Name = bigQueryName,
                Id = bigQueryId
            };
        }
    }
}
