using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public abstract class ATSPMRepositoryBQBase<T> : IAsyncRepository<T> where T : class
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

        public IQueryable<T> GetList()
        {
            throw new NotImplementedException("BigQuery does not support IQueryable directly. Use GetListAsync instead.");
        }

        public IReadOnlyList<T> GetList(Expression<Func<T, bool>> criteria)
        {
            return GetListAsync(criteria).Result;
        }

        public IReadOnlyList<T> GetList(ISpecification<T> criteria)
        {
            return GetListAsync(criteria).Result;
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            var sqlWhereClause = new SqlExpressionVisitor().Translate(criteria);
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE {sqlWhereClause}";
            var results = await _client.ExecuteQueryAsync(query, parameters: null);
            return results.Select(row => MapRowToEntity(row)).ToList();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria)
        {
            var sqlWhereClause = new SqlExpressionVisitor().Translate(criteria.Criteria);
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE {sqlWhereClause}";
            var results = await _client.ExecuteQueryAsync(query, parameters: null);
            return results.Select(row => MapRowToEntity(row)).ToList();
        }

        public T Lookup(object key)
        {
            return LookupAsync(key).Result;
        }

        public T Lookup(T item)
        {
            throw new NotImplementedException("Lookup by item is not implemented.");
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

        public Task<T> LookupAsync(T item)
        {
            throw new NotImplementedException("Lookup by item is not implemented.");
        }

        public void Add(T item)
        {
            AddAsync(item).Wait();
        }

        public async Task AddAsync(T item)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            var insertRow = CreateRow(item);
            await table.InsertRowAsync(insertRow);
        }

        public void AddRange(IEnumerable<T> items)
        {
            AddRangeAsync(items).Wait();
        }

        public async Task AddRangeAsync(IEnumerable<T> items)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            var rows = items.Select(CreateRow);
            await table.InsertRowsAsync(rows);
        }

        public void Remove(T item)
        {
            RemoveAsync(GetPrimaryKey(item)).Wait();
        }

        public async Task RemoveAsync(T item)
        {
            var key = GetPrimaryKey(item);
            await RemoveAsync(key);
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

        public void RemoveRange(IEnumerable<T> items)
        {
            RemoveRangeAsync(items).Wait();
        }

        public async Task RemoveRangeAsync(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                await RemoveAsync(GetPrimaryKey(item));
            }
        }

        public void Update(T item)
        {
            UpdateAsync(item).Wait();
        }

        public async Task UpdateAsync(T item)
        {
            var key = GetPrimaryKey(item);
            var updateRow = CreateRow(item);
            var query = $"UPDATE `{_datasetId}.{_tableId}` SET {GenerateUpdateQuery(updateRow)} WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                new BigQueryParameter("key", BigQueryDbType.String, key) // Adjust BigQueryDbType as needed
            };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        public void UpdateRange(IEnumerable<T> items)
        {
            UpdateRangeAsync(items).Wait();
        }

        public async Task UpdateRangeAsync(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }

        #endregion

        protected abstract BigQueryInsertRow CreateRow(T item);

        protected abstract T MapRowToEntity(BigQueryRow row);

        protected abstract object GetPrimaryKey(T item);

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
    }

    public class SqlExpressionVisitor : ExpressionVisitor
    {
        private StringBuilder _sql;

        public string Translate(Expression expression)
        {
            _sql = new StringBuilder();
            Visit(expression);
            return _sql.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _sql.Append("(");
            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    _sql.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    _sql.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    _sql.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    _sql.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    _sql.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _sql.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    _sql.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _sql.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException($"The binary operator {node.NodeType} is not supported");
            }

            Visit(node.Right);
            _sql.Append(")");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                _sql.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(node.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _sql.Append((bool)node.Value ? 1 : 0);
                        break;
                    case TypeCode.String:
                        _sql.Append($"'{node.Value}'");
                        break;
                    case TypeCode.DateTime:
                        _sql.Append($"'{node.Value:yyyy-MM-dd HH:mm:ss}'");
                        break;
                    default:
                        _sql.Append(node.Value);
                        break;
                }
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _sql.Append(node.Member.Name);
            return node;
        }
    }
}
