using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories
{
    public class SegmentEntityBQRepository : ATSPMRepositoryBQBase<SegmentEntity>, ISegmentEntityRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<SegmentEntity>> _logger;
        private BigQueryTable _table;

        public SegmentEntityBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<SegmentEntity>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
            _table = _client.GetTable(_datasetId, _tableId);
        }

        public void Add(SegmentEntity item)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(SegmentEntity item)
        {
            throw new NotImplementedException();
        }

        public async Task AddEntitiesAsync(List<SegmentEntity> routeEntities)
        {
            List<BigQueryInsertRow> insertRows = new List<BigQueryInsertRow>();
            int batchSize = 10000;

            foreach (var routeEntity in routeEntities)
            {
                insertRows.Add(CreateRow(routeEntity));

                if (insertRows.Count == batchSize)
                {
                    await _table.InsertRowsAsync(insertRows);
                    insertRows.Clear(); // Clear the list for the next batch
                }
            }

            // Insert any remaining rows that didn't make a full batch
            if (insertRows.Count > 0)
            {
                await _table.InsertRowsAsync(insertRows);
            }
        }

        public void AddRange(IEnumerable<SegmentEntity> items)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<SegmentEntity> items)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SegmentEntity>> GetEntitiesForEntityType(string type)
        {
            string query = $@"
                            SELECT 
                                entityTable.*,
                                route.SpeedLimit
                            FROM 
                                `atspm-406601.speedDataset.segmentEntity` as entity_table
                            JOIN `atspm-406601.speedDataset.segment` as route
                            ON 
                                entity_table.SegmentId = route.Id
                            WHERE 
                                entity_table.EntityType LIKE @entityType";

            var parameters = new[]
                {
                    new BigQueryParameter("entityType", BigQueryDbType.String, type),
                };
            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            List<SegmentEntity> results = new List<SegmentEntity>();
            foreach (BigQueryRow row in queryResults)
            {
                var segmentId = Guid.Parse(row["SegmentId"].ToString());
                var entityId = row["EntityId"];
                var entityType = row["EntityType"];
                var sourceId = row["SourceId"];

                var result = new SegmentEntity
                {
                    EntityId = entityId.ToString(),
                    EntityType = entityType.ToString(),
                    SegmentId = segmentId,
                    SourceId = (long)sourceId
                };

                results.Add(result);
            }

            return results;
        }

        public async Task<List<SegmentEntityWithSpeedAndAlternateIdentifier>> GetEntitiesWithSpeedAndAlternateIdentifierForSourceId(int id)
        {
            string query = $@"
                             SELECT 
                                entityTable.*,
                                route.SpeedLimit,
                                route.AlternateIdentifier
                            FROM 
                                `atspm-406601.speedDataset.segmentEntity` as entityTable
                            JOIN
                                `atspm-406601.speedDataset.segment` as route
                            ON 
                                entityTable.SegmentId = route.Id
                            WHERE 
                                entityTable.SourceId = @sourceId";
            var parameters = new[]
                        {
                            new BigQueryParameter("sourceId", BigQueryDbType.Int64, id),
                        };
            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            List<SegmentEntityWithSpeedAndAlternateIdentifier> results = new List<SegmentEntityWithSpeedAndAlternateIdentifier>();
            foreach (BigQueryRow row in queryResults)
            {
                var segmentId = Guid.Parse(row["SegmentId"].ToString());
                var entityId = row["EntityId"];
                var entityType = row["EntityType"];
                var sourceId = row["SourceId"];
                var speedLimit = row["SpeedLimit"];
                var alternateIdentifier = row["AlternateIdentifier"];
                if (alternateIdentifier == null)
                {
                    //TODO - Log
                    continue;
                }

                var result = new SegmentEntityWithSpeedAndAlternateIdentifier
                {
                    EntityId = entityId.ToString(),
                    EntityType = entityType.ToString(),
                    SegmentId = segmentId,
                    SourceId = (long)sourceId,
                    SpeedLimit = (long)speedLimit,
                    AlternateIdentifier = alternateIdentifier.ToString()
                };

                results.Add(result);
            }

            return results;
        }

        public async Task<List<SegmentEntityWithSpeed>> GetEntitiesWithSpeedForSourceId(int id)
        {
            string query = $@"
                             SELECT 
                                entity_table.*,
                                route.SpeedLimit
                            FROM 
                                `atspm-406601.speedDataset.segmentEntity` as entity_table
                            JOIN
                                `atspm-406601.speedDataset.segment` as route
                            ON 
                                entity_table.SegmentId = route.Id
                            WHERE 
                                entity_table.SourceId = @sourceId";
            var parameters = new[]
                        {
                            new BigQueryParameter("sourceId", BigQueryDbType.Int64, id),
                        };
            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            List<SegmentEntityWithSpeed> results = new List<SegmentEntityWithSpeed>();
            foreach (BigQueryRow row in queryResults)
            {
                var segmentId = Guid.Parse(row["SegmentId"].ToString());
                var entityId = row["EntityId"];
                var entityType = row["EntityType"];
                var sourceId = row["SourceId"];
                var speedLimit = row["SpeedLimit"];
                var length = row["Length"];

                var result = new SegmentEntityWithSpeed
                {
                    EntityId = entityId.ToString(),
                    EntityType = entityType.ToString(),
                    SegmentId = segmentId,
                    SourceId = (long)sourceId,
                    SpeedLimit = (long)speedLimit,
                    Length = length != null ? (double)length : 0.0,
                };

                results.Add(result);
            }

            return results;
        }

        public override IQueryable<SegmentEntity> GetList()
        {
            throw new NotImplementedException();
        }

        public override SegmentEntity Lookup(object key)
        {
            throw new NotImplementedException();
        }

        public override SegmentEntity Lookup(SegmentEntity item)
        {
            throw new NotImplementedException();
        }

        public override Task<SegmentEntity> LookupAsync(object key)
        {
            throw new NotImplementedException();
        }

        public override Task<SegmentEntity> LookupAsync(SegmentEntity item)
        {
            throw new NotImplementedException();
        }

        public override void Remove(SegmentEntity item)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveAsync(SegmentEntity item)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRange(IEnumerable<SegmentEntity> items)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveRangeAsync(IEnumerable<SegmentEntity> items)
        {
            throw new NotImplementedException();
        }

        public override void Update(SegmentEntity item)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateAsync(SegmentEntity item)
        {
            throw new NotImplementedException();
        }

        public override void UpdateRange(IEnumerable<SegmentEntity> items)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateRangeAsync(IEnumerable<SegmentEntity> items)
        {
            throw new NotImplementedException();
        }

        protected override BigQueryInsertRow CreateRow(SegmentEntity item)
        {
            return new BigQueryInsertRow
            {
                {"SegmentId", item.SegmentId },
                {"EntityId", item.EntityId },
                {"EntityType", item.EntityType },
                {"SourceId", item.SourceId },
            };
        }

        protected override SegmentEntity MapRowToEntity(BigQueryRow row)
        {
            throw new NotImplementedException();
        }
    }
}
