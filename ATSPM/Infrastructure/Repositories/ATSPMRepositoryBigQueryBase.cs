using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public abstract class ATSPMRepositoryBQBase<T> where T : class
    {
        protected readonly ILogger _log;
        protected readonly BigQueryClient _client;
        protected readonly string _datasetId;
        protected readonly string _tableId;

        public ATSPMRepositoryBQBase(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<T>> log)
        {
            _log = log;
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
        }

        #region IAsyncRepository

        public async Task AddAsync(T item)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            var insertRow = CreateRow(item);
            await table.InsertRowAsync(insertRow);
        }

        public async Task AddRangeAsync(IEnumerable<T> items)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            var rows = items.Select(CreateRow);
            await table.InsertRowsAsync(rows);
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE {TranslateExpressionToSQL(criteria)}";
            var results = await _client.ExecuteQueryAsync(query, parameters: null);
            return results.Select(row => MapRowToEntity(row)).ToList();
        }

        public async Task<T> LookupAsync(object key)
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key"; // Assuming primary key column is named "Id"
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("key", BigQueryDbType.String, key) // Adjust BigQueryDbType as needed
            };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public async Task RemoveAsync(object key)
        {
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key"; // Assuming primary key column is named "Id"
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("key", BigQueryDbType.String, key) // Adjust BigQueryDbType as needed
            };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        public async Task UpdateAsync(T item, object key)
        {
            var updateRow = CreateRow(item);
            var query = $"UPDATE `{_datasetId}.{_tableId}` SET {GenerateUpdateQuery(updateRow)} WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("key", BigQueryDbType.String, key) // Adjust BigQueryDbType as needed
            };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        #endregion

        protected abstract BigQueryInsertRow CreateRow(T item);

        protected abstract T MapRowToEntity(BigQueryRow row);

        private string GenerateUpdateQuery(BigQueryInsertRow row)
        {
            var updateColumns = new List<string>();
            foreach (var field in row)
            {
                var kvp = (KeyValuePair<string, object>)field;
                updateColumns.Add($"{kvp.Key} = @{kvp.Key}");
            }
            return string.Join(", ", updateColumns);
        }

        private string TranslateExpressionToSQL(Expression<Func<T, bool>> expression)
        {
            // Implement a method to translate LINQ expression to SQL WHERE clause
            throw new NotImplementedException();
        }
    }
}
