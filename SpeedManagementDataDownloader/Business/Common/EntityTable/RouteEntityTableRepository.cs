using Google.Cloud.BigQuery.V2;
using SpeedManagementDataDownloader.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Common.EntityTable
{
    public class RouteEntityTableRepository : IRouteEntityTableRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private BigQueryTable _table;

        public RouteEntityTableRepository(BigQueryClient client, string datasetId, string tableId)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _table = _client.GetTable(_datasetId, _tableId);
        }

        public async Task AddEntitiesAsync(List<RouteEntityTable> routeEntities)
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
            throw new NotImplementedException();
        }

        protected BigQueryInsertRow CreateRow(RouteEntityTable item)
        {
            return new BigQueryInsertRow
            {
                {"RouteId", item.RouteId },
                {"EntityId", item.EntityId },
                {"EntityType", item.EntityType },
                {"SourceId", item.SourceId },
            };
        }

        public async Task GetEntitiesForEntityType(string type)
        {
            string query = $@"
                            SELECT 
                                entity_table.*,
                                route.SpeedLimit
                            FROM 
                                `atspm-406601.speed_dataset.entity_table` as entity_table
                            JOIN `atspm-406601.speed_dataset.route` as route
                            ON 
                                entity_table.RouteId = route.Id
                            WHERE 
                                entity_table.EntityType LIKE @entityType";

            var parameters = new[]
                        {
                            new BigQueryParameter("entityType", BigQueryDbType.String, type),
                        };
            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            List<RouteEntityTable> results = new List<RouteEntityTable>();
            foreach (BigQueryRow row in queryResults)
            {
                var routeId = row["RouteId"];
                var entityId = row["EntityId"];
                var entityType = row["EntityType"];
                var sourceId = row["SourceId"];

                var result = new RouteEntityTable
                {
                    EntityId = (int)entityId,
                    EntityType = entityType.ToString(),
                    RouteId = (int)routeId,
                    SourceId = sourceId.ToString()
                };
            }
        }

        public async Task<List<RouteEntityWithSpeed>> GetEntitiesWithSpeedForSourceId(int id)
        {
            string query = $@"
                             SELECT 
                                entity_table.*,
                                route.SpeedLimit
                            FROM 
                                `atspm-406601.speed_dataset.route_entity` as entity_table
                            JOIN
                                `atspm-406601.speed_dataset.route` as route
                            ON 
                                entity_table.RouteId = route.Id
                            WHERE 
                                entity_table.SourceId = @sourceId";
            var parameters = new[]
                        {
                            new BigQueryParameter("sourceId", BigQueryDbType.Int64, id),
                        };
            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            List<RouteEntityWithSpeed> results = new List<RouteEntityWithSpeed>();
            foreach (BigQueryRow row in queryResults)
            {
                var routeId = row["RouteId"];
                var entityId = row["EntityId"];
                var entityType = row["EntityType"];
                var sourceId = row["SourceId"];
                var speedLimit = row["SpeedLimit"];

                var result = new RouteEntityWithSpeed
                {
                    EntityId = (long)entityId,
                    EntityType = entityType.ToString(),
                    RouteId = (long)routeId,
                    SourceId = (long)sourceId,
                    SpeedLimit = (long)speedLimit
                };

                results.Add(result);
            }

            return results;
        }

        public async Task<List<RouteEntityWithSpeedAndAlternateIdentifier>> GetEntitiesWithSpeedAndAlternateIdentifierForSourceId(int id)
        {
            string query = $@"
                             SELECT 
                                entity_table.*,
                                route.SpeedLimit,
                                route.AlternateIdentifier
                            FROM 
                                `atspm-406601.speed_dataset.entity_table` as entity_table
                            JOIN
                                `atspm-406601.speed_dataset.route` as route
                            ON 
                                entity_table.RouteId = route.Id
                            WHERE 
                                entity_table.SourceId LIKE @sourceId";
            var parameters = new[]
                        {
                            new BigQueryParameter("sourceId", BigQueryDbType.String, id),
                        };
            var queryResults = await _client.ExecuteQueryAsync(query, parameters);
            List<RouteEntityWithSpeedAndAlternateIdentifier> results = new List<RouteEntityWithSpeedAndAlternateIdentifier>();
            foreach (BigQueryRow row in queryResults)
            {
                var routeId = row["RouteId"];
                var entityId = row["EntityId"];
                var entityType = row["EntityType"];
                var sourceId = row["SourceId"];
                var speedLimit = row["SpeedLimit"];
                var alternateIdentifier = row["AlternateIdentifier"];

                var result = new RouteEntityWithSpeedAndAlternateIdentifier
                {
                    EntityId = (int)entityId,
                    EntityType = entityType.ToString(),
                    RouteId = (int)routeId,
                    SourceId = sourceId.ToString(),
                    SpeedLimit = (int)speedLimit,
                    AlternateIdentifier = alternateIdentifier.ToString()
                };

                results.Add(result);
            }

            return results;
        }

    }
}
