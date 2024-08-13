using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
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
    public class ImpactBQRepository : ATSPMRepositoryBQBase<Impact>, IImpactRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<Impact>> _logger;

        public ImpactBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<Impact>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
        }

        public override IQueryable<Impact> GetList()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = _client.ExecuteQuery(query, parameters).ToList();

            return result.Select(row => MapRowToEntity(row)).ToList().AsQueryable();
        }

        public override Impact Lookup(object key)
        {
            if (key == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key.ToString())
                };
            var results = _client.ExecuteQuery(query, parameters);
            Task<Impact> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override Impact Lookup(Impact item)
        {
            if (item.Id == null) return null;
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
                };

            var results = _client.ExecuteQuery(query, parameters);
            Task<Impact> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override async Task<Impact> LookupAsync(object key)
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

        public override async Task<Impact> LookupAsync(Impact item)
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

        public override void Remove(Impact item)
        {
            if (item.Id == null) return;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
             };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override async Task RemoveAsync(Impact item)
        {
            if (item.Id == null) return;
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
             };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<Impact> items)
        {
            var ids = string.Join(", ", items.Select(i => i.Id));
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>
            {
                 new BigQueryParameter("ids", BigQueryDbType.String, ids)
             };

            _client.ExecuteQuery(query, parameters);
        }

        public override async Task RemoveRangeAsync(IEnumerable<Impact> items)
        {
            var ids = string.Join(", ", items.Select(i => i.Id));
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("ids", BigQueryDbType.String, ids)
             };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override async void Update(Impact item)
        {
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



        public async Task<Impact> UpdateImpactAsync(Impact item)
        {
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {

                var (query, parameters) = UpdateQuery(item);

                var result = await _client.ExecuteQueryAsync(query, parameters);
                return await LookupAsync(item.Id);
            }
            else
            {
                var (query, id) = InsertQuery(item);

                var parameters = new List<BigQueryParameter>();

                var result = await _client.ExecuteQueryAsync(query, parameters);
                return await LookupAsync(id);
            }
        }

        public override async Task UpdateAsync(Impact item)
        {
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                var (query, parameters) = UpdateQuery(item);

                await _client.ExecuteQueryAsync(query, parameters);
            }
            else
            {
                var (query, id) = InsertQuery(item);

                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
            }
        }

        public override void UpdateRange(IEnumerable<Impact> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public override async Task UpdateRangeAsync(IEnumerable<Impact> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }

        protected override BigQueryInsertRow CreateRow(Impact item)
        {
            return new BigQueryInsertRow
            {
                { "Id", item.Id },
                { "Description", item.Description },
                { "Start", item.Start },
                { "End", item.End },
                { "StartMile", item.StartMile },
                { "EndMile", item.EndMile },
                { "CreatedOn", item.CreatedOn },
                { "CreatedBy", item.CreatedBy.ToString() },
                { "UpdatedOn", item.UpdatedOn },
                { "UpdatedBy", item.UpdatedBy.ToString() },
                { "DeletedOn", item.DeletedOn },
                { "DeletedBy", item.DeletedBy.ToString() }
            };
        }

        protected override Impact MapRowToEntity(BigQueryRow row)
        {

            var bigQueryId = Guid.Parse(row["Id"].ToString());
            var bigQueryDescription = row["Description"].ToString();
            var bigQueryStart = DateTime.Parse(row["Start"].ToString());
            var bigQueryEnd = row["End"] != null ? DateTime.Parse(row["End"].ToString()) : (DateTime?)null;
            double bigQueryStartMile = double.Parse(row["StartMile"].ToString());
            var bigQueryEndMile = double.Parse(row["EndMile"].ToString());
            var bigQueryCreatedOn = DateTime.Parse(row["CreatedOn"].ToString()); //DateTime
            var bigQueryCreatedBy = row["CreatedBy"].ToString();
            var bigQueryUpdatedOn = row["UpdatedOn"] != null ? DateTime.Parse(row["UpdatedOn"].ToString()) : (DateTime?)null; //DateTime?
            var bigQueryUpdatedBy = row["UpdatedBy"]?.ToString();
            var bigQueryDeletedOn = row["DeletedOn"] != null ? DateTime.Parse(row["DeletedOn"].ToString()) : (DateTime?)null; //DateTime?
            var bigQueryDeletedBy = row["DeletedBy"]?.ToString();

            return new Impact
            {
                Id = bigQueryId,
                Description = bigQueryDescription,
                Start = bigQueryStart,
                End = bigQueryEnd,
                StartMile = bigQueryStartMile,
                EndMile = bigQueryEndMile,
                CreatedOn = bigQueryCreatedOn,
                CreatedBy = bigQueryCreatedBy,
                UpdatedOn = bigQueryUpdatedOn,
                UpdatedBy = bigQueryUpdatedBy,
                DeletedOn = bigQueryDeletedOn,
                DeletedBy = bigQueryDeletedBy
            };
        }

        public async Task<List<Impact>> GetInstancesDetails(List<Guid> impactIds)
        {
            // Construct a comma-separated list of IDs for the IN clause
            string ids = string.Join(",", impactIds.Select(id => $"'{id}'"));

            // Query with IN clause
            string query = $@"
                SELECT * 
                FROM `{_datasetId}.{_tableId}` 
                WHERE Id IN ({ids});";

            var parameters = new List<BigQueryParameter>();

            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).ToList();
        }


        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

        private Tuple<string, List<BigQueryParameter>> UpdateQuery(Impact item)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");

            if (!string.IsNullOrEmpty(item.Description))
            {
                queryBuilder.Append($"Description = '{item.Description}', ");
            }

            if (item.Start != DateTime.MinValue)
            {
                queryBuilder.Append($"Start = DATETIME('{item.Start:yyyy-MM-dd HH:mm:ss}'), ");
            }

            if (item.End.HasValue)
            {
                queryBuilder.Append($"`End` = DATETIME('{item.End.Value:yyyy-MM-dd HH:mm:ss}'), ");
            }
            else
            {
                queryBuilder.Append($"`End` = NULL, ");
            }

            queryBuilder.Append($"StartMile = {item.StartMile}, ");
            queryBuilder.Append($"EndMile = {item.EndMile}, ");
            queryBuilder.Append($"UpdatedOn = DATETIME('{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}'), ");
            queryBuilder.Append($"UpdatedBy = {(string.IsNullOrEmpty(item.UpdatedBy) ? "NULL" : $"'{item.UpdatedBy}'")}, ");
            queryBuilder.Append($"DeletedOn = {(item.DeletedOn.HasValue ? $"DATETIME('{item.DeletedOn.Value:yyyy-MM-dd HH:mm:ss}')" : "NULL")}, ");
            queryBuilder.Append($"DeletedBy = {(string.IsNullOrEmpty(item.DeletedBy) ? "NULL" : $"'{item.DeletedBy}'")}, ");

            // Remove the last comma and space
            queryBuilder.Length -= 2;

            queryBuilder.Append(" WHERE Id = @key");

            var query = queryBuilder.ToString();
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
                };

            return new Tuple<string, List<BigQueryParameter>>(query, parameters);
        }


        private Tuple<string, Guid> InsertQuery(Impact item)
        {
            Guid id = Guid.NewGuid();
            var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                "(Id, Description, Start, `End`, StartMile, EndMile, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy, DeletedOn, DeletedBy) " +
                "VALUES (" +
                $"'{id}', " +
                $"'{item.Description}', " +
                $"DATETIME('{item.Start:yyyy-MM-dd HH:mm:ss}'), " +
                $"{(item.End.HasValue ? $"DATETIME('{item.End.Value:yyyy-MM-dd HH:mm:ss}')" : "NULL")}, " +
                $"{item.StartMile}, " +
                $"{item.EndMile}, " +
                $"DATETIME('{item.CreatedOn:yyyy-MM-dd HH:mm:ss}'), " +
                $"'{item.CreatedBy}', " +
                "NULL, " +  // UpdatedOn is set to NULL
                "NULL, " +  // UpdatedBy is set to NULL
                "NULL, " +  // DeletedOn is set to NULL
                "NULL)";  // DeletedBy is set to NULL

            return new Tuple<string, Guid>(query, id);
        }

    }
}
