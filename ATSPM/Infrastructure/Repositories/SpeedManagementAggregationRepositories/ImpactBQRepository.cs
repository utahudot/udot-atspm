using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Extensions;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementAggregationRepositories
{
    ///<inheritdoc cref="IRouteRepository"/>
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

            // Map the result to a list of ImpactType objects
            return result.Select(row => MapRowToEntity(row)).ToList().AsQueryable();
        }

        public override Impact Lookup(object key)
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.Int64, key)
                };
            var results = _client.ExecuteQuery(query, parameters);
            Task<Impact> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override Impact Lookup(Impact item)
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.Int64, item.Id)
                };

            var results = _client.ExecuteQuery(query, parameters);
            Task<Impact> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override async Task<Impact> LookupAsync(object key)
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.Int64, key)
                };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override async Task<Impact> LookupAsync(Impact item)
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.Int64, item.Id)
                };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override void Remove(Impact item)
        {
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.Int64, item)
             };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override async Task RemoveAsync(Impact item)
        {
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.Int64, item)
             };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<Impact> items)
        {
            var ids = string.Join(", ", items);
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>();

            _client.ExecuteQuery(query, parameters);
        }

        public override async Task RemoveRangeAsync(IEnumerable<Impact> items)
        {
            var ids = string.Join(", ", items);
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>();

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override async void Update(Impact item)
        {
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {

                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");

                if (!string.IsNullOrEmpty(item.Description))
                {
                    queryBuilder.Append($"Description = '{item.Description}', ");
                }

                if (item.Start != DateTime.MinValue)
                {
                    queryBuilder.Append($"Start = '{item.Start:O}', ");
                }

                if (item.End.HasValue)
                {
                    queryBuilder.Append($"End = '{item.End:O}', ");
                }

                queryBuilder.Append($"StartMile = '{item.StartMile}', ");
                queryBuilder.Append($"EndMile = '{item.EndMile}', ");
                queryBuilder.Append($"Shape = '{item.Shape.AsText()}', ");
                queryBuilder.Append($"ImpactTypeId = {item.ImpactTypeId}, ");
                queryBuilder.Append($"UpdatedOn = '{DateTime.UtcNow:O}', ");
                queryBuilder.Append($"UpdatedBy = {(item.UpdatedBy.HasValue ? $"'{item.UpdatedBy}'" : "NULL")}, ");
                queryBuilder.Append($"DeletedOn = {(item.DeletedOn.HasValue ? $"'{item.DeletedOn:O}'" : "NULL")}, ");
                queryBuilder.Append($"DeletedBy = {(item.DeletedBy.HasValue ? $"'{item.DeletedBy}'" : "NULL")}, ");
                // Remove the last comma and space
                queryBuilder.Length -= 2;

                queryBuilder.Append($" WHERE Id = @key");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQueryAsync(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Description, Start, End, StartMile, EndMile, Shape, ImpactTypeId, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy, DeletedOn, DeletedBy) " +
                    $"VALUES (" +
                    $"'{item.Description}', " +
                    $"'{item.Start:O}', " +
                    $"{(item.End.HasValue ? $"'{item.End:O}'" : "NULL")}, " +
                    $"'{item.StartMile}', " +
                    $"'{item.EndMile}', " +
                    $"'{item.Shape.AsText()}', " +
                    $"{item.ImpactTypeId}, " +
                    $"'{item.CreatedOn:O}', " +
                    $"'{item.CreatedBy}', " +
                    $"NULL, " +  // UpdatedOn is set to NULL
                    $"NULL, " +  // UpdatedBy is set to NULL
                    $"NULL, " +  // DeletedOn is set to NULL
                    $"NULL)";    // DeletedBy is set to NULL
                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQueryAsync(query, parameters);
            }
        }

        public override async Task UpdateAsync(Impact item)
        {
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {

                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");

                if (!string.IsNullOrEmpty(item.Description))
                {
                    queryBuilder.Append($"Description = '{item.Description}', ");
                }

                if (item.Start != DateTime.MinValue)
                {
                    queryBuilder.Append($"Start = '{item.Start:O}', ");
                }

                if (item.End.HasValue)
                {
                    queryBuilder.Append($"End = '{item.End:O}', ");
                }

                queryBuilder.Append($"StartMile = '{item.StartMile}', ");
                queryBuilder.Append($"EndMile = '{item.EndMile}', ");
                queryBuilder.Append($"Shape = '{item.Shape.AsText()}', ");
                queryBuilder.Append($"ImpactTypeId = {item.ImpactTypeId}, ");
                queryBuilder.Append($"UpdatedOn = '{DateTime.UtcNow:O}', ");
                queryBuilder.Append($"UpdatedBy = {(item.UpdatedBy.HasValue ? $"'{item.UpdatedBy}'" : "NULL")}, ");
                queryBuilder.Append($"DeletedOn = {(item.DeletedOn.HasValue ? $"'{item.DeletedOn:O}'" : "NULL")}, ");
                queryBuilder.Append($"DeletedBy = {(item.DeletedBy.HasValue ? $"'{item.DeletedBy}'" : "NULL")}, ");
                // Remove the last comma and space
                queryBuilder.Length -= 2;

                queryBuilder.Append($" WHERE Id = @key");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Description, Start, End, StartMile, EndMile, Shape, ImpactTypeId, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy, DeletedOn, DeletedBy) " +
                    $"VALUES (" +
                    $"'{item.Description}', " +
                    $"'{item.Start:O}', " +
                    $"{(item.End.HasValue ? $"'{item.End:O}'" : "NULL")}, " +
                    $"'{item.StartMile}', " +
                    $"'{item.EndMile}', " +
                    $"'{item.Shape.AsText()}', " +
                    $"{item.ImpactTypeId}, " +
                    $"'{item.CreatedOn:O}', " +
                    $"'{item.CreatedBy}', " +
                    $"NULL, " +  // UpdatedOn is set to NULL
                    $"NULL, " +  // UpdatedBy is set to NULL
                    $"NULL, " +  // DeletedOn is set to NULL
                    $"NULL)";    // DeletedBy is set to NULL
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
                { "Shape", item.Shape },  // Assuming you have a proper string representation for Geometry
                { "ImpactTypeId", item.ImpactTypeId },
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
            var wktReader = new WKTReader();

            return new Impact
            {
                Id = row.GetPropertyValue<int>("Id"),
                Description = row.GetPropertyValue<string>("Description"),
                Start = row.GetPropertyValue<DateTime>("Start"),
                End = row.GetPropertyValue<DateTime?>("End"),
                StartMile = row.GetPropertyValue<string>("StartMile"),
                EndMile = row.GetPropertyValue<string>("EndMile"),
                Shape = wktReader.Read(row.GetPropertyValue<string>("Shape")),  // Assuming a FromString method in Geometry class
                ImpactTypeId = row.GetPropertyValue<int>("ImpactTypeId"),
                CreatedOn = row.GetPropertyValue<DateTime>("CreatedOn"),
                CreatedBy = row.GetPropertyValue<Guid>("CreatedBy"),
                UpdatedOn = row.GetPropertyValue<DateTime?>("UpdatedOn"),
                UpdatedBy = row.GetPropertyValue<Guid>("UpdatedBy"),
                DeletedOn = row.GetPropertyValue<DateTime?>("DeletedOn"),
                DeletedBy = row.GetPropertyValue<Guid>("DeletedBy")
            };
        }
    }
}
