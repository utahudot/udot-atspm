using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementRepositories
{
    ///<inheritdoc cref="ISegmentRepository"/>
    public class SegmentBQRepository : ATSPMRepositoryBQBase<Segment>, ISegmentRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly string _projectId;
        private readonly ILogger<ATSPMRepositoryBQBase<Segment>> _logger;

        public SegmentBQRepository(BigQueryClient client, string datasetId, string tableId, string projectId, ILogger<ATSPMRepositoryBQBase<Segment>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _projectId = projectId;
            _logger = log;
        }

        public async Task AddRoutesAsync(IEnumerable<Segment> routes)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            List<BigQueryInsertRow> insertRows = new List<BigQueryInsertRow>();
            foreach (var route in routes)
            {
                insertRows.Add(CreateRow(route));
            }
            await table.InsertRowsAsync(insertRows);
        }

        public async Task AddRouteAsync(Segment route)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            var insertRow = CreateRow(route);
            await table.InsertRowAsync(insertRow);
        }

        protected override BigQueryInsertRow CreateRow(Segment route)
        {
            return new BigQueryInsertRow
            {
                { "Id", route.Id },
                { "UdotRouteNumber", route.UdotRouteNumber },
                { "StartMilePoint", route.StartMilePoint },
                { "EndMilePoint", route.EndMilePoint },
                { "FunctionalType", route.FunctionalType },
                { "Name", route.Name },
                { "Direction", route.Direction },
                { "SpeedLimit", route.SpeedLimit },
                { "Region", route.Region },
                { "City", route.City },
                { "County", route.County },
                { "Shape", route.ShapeWKT }, // Assuming ShapeWKT is used for GEOGRAPHY
                { "ShapeWKT", route.ShapeWKT },
                //{ "AlternateIdentifier", route.AlternateIdentifier ?? null }
            };
        }

        public override Segment Lookup(object key)
        {
            if (key == null)
            {
                return null;
            }
            var query = $"SELECT * FROM `{_projectId}.{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key.ToString())
                };
            var results = _client.ExecuteQuery(query, parameters);
            Task<Segment> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override Segment Lookup(Segment item)
        {
            if (item.Id == null)
            {
                return null;
            }
            var query = $"SELECT * FROM `{_projectId}.{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
                };

            var results = _client.ExecuteQuery(query, parameters);
            Task<Segment> task = Task.FromResult(results.Select(row => MapRowToEntity(row)).FirstOrDefault());
            return task.Result;
        }

        public override async Task<Segment> LookupAsync(object key)
        {
            if (key == null)
            {
                return null;
            }
            var query = $"SELECT * FROM `{_projectId}.{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
            {
                    new BigQueryParameter("key", BigQueryDbType.String, key.ToString())
            };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override async Task<Segment> LookupAsync(Segment item)
        {
            if (item.Id == null)
            {
                return null;
            }
            var query = $"SELECT * FROM `{_projectId}.{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
                {
                    new BigQueryParameter("key", BigQueryDbType.String, item.Id.ToString())
                };
            var results = await _client.ExecuteQueryAsync(query, parameters);
            return results.Select(row => MapRowToEntity(row)).FirstOrDefault();
        }

        public override void Remove(Segment item)
        {
            if (item.Id == null)
            {
                return;
            }
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.ToString())
             };
            _client.ExecuteQueryAsync(query, parameters);
        }

        public override async Task RemoveAsync(Segment item)
        {
            if (item.Id == null)
            {
                return;
            }
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id = @key";
            var parameters = new List<BigQueryParameter>
             {
                 new BigQueryParameter("key", BigQueryDbType.String, item.ToString())
             };
            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override void RemoveRange(IEnumerable<Segment> items)
        {
            var ids = string.Join(", ", items);
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>
            {
                 new BigQueryParameter("ids", BigQueryDbType.String, ids)
             };

            _client.ExecuteQuery(query, parameters);
        }

        public override async Task RemoveRangeAsync(IEnumerable<Segment> items)
        {
            var ids = string.Join(", ", items);
            var query = $"DELETE FROM `{_datasetId}.{_tableId}` WHERE Id IN ({ids})";
            var parameters = new List<BigQueryParameter>
            {
                 new BigQueryParameter("ids", BigQueryDbType.String, ids)
             };

            await _client.ExecuteQueryAsync(query, parameters);
        }

        public override async void Update(Segment item)
        {
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");

                queryBuilder.Append($"UdotRouteNumber = {item.UdotRouteNumber}, ");
                queryBuilder.Append($"StartMilePoint = {item.StartMilePoint}, ");
                queryBuilder.Append($"EndMilePoint = {item.EndMilePoint}, ");

                if (!string.IsNullOrEmpty(item.FunctionalType))
                {
                    queryBuilder.Append($"FunctionalType = '{item.FunctionalType}', ");
                }

                if (!string.IsNullOrEmpty(item.Name))
                {
                    queryBuilder.Append($"Name = '{item.Name}', ");
                }

                if (!string.IsNullOrEmpty(item.Direction))
                {
                    queryBuilder.Append($"Direction = '{item.Direction}', ");
                }

                queryBuilder.Append($"SpeedLimit = {item.SpeedLimit}, ");

                if (!string.IsNullOrEmpty(item.Region))
                {
                    queryBuilder.Append($"Region = '{item.Region}', ");
                }

                if (!string.IsNullOrEmpty(item.City))
                {
                    queryBuilder.Append($"City = '{item.City}', ");
                }

                if (!string.IsNullOrEmpty(item.County))
                {
                    queryBuilder.Append($"County = '{item.County}', ");
                }

                queryBuilder.Append($"Shape = '{item.Shape.AsText()}', ");
                queryBuilder.Append($"ShapeWKT = '{item.ShapeWKT}', ");

                if (!string.IsNullOrEmpty(item.AlternateIdentifier))
                {
                    queryBuilder.Append($"AlternateIdentifier = '{item.AlternateIdentifier}', ");
                }

                // Remove the last comma and space
                queryBuilder.Length -= 2;

                queryBuilder.Append($" WHERE Id = @key");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@key", BigQueryDbType.String, item.Id.ToString())
        };

                _client.ExecuteQuery(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Id, UdotRouteNumber, StartMilePoint, EndMilePoint, FunctionalType, Name, Direction, SpeedLimit, Region, City, County, Shape, ShapeWKT, AlternateIdentifier) " +
                    $"VALUES (" +
                    $"{item.Id}, " +
                    $"{item.UdotRouteNumber}, " +
                    $"{item.StartMilePoint}, " +
                    $"{item.EndMilePoint}, " +
                    $"'{item.FunctionalType}', " +
                    $"'{item.Name}', " +
                    $"'{item.Direction}', " +
                    $"{item.SpeedLimit}, " +
                    $"'{item.Region}', " +
                    $"'{item.City}', " +
                    $"'{item.County}', " +
                    $"'{item.Shape.AsText()}', " +
                    $"'{item.ShapeWKT}', " +
                    $"{(string.IsNullOrEmpty(item.AlternateIdentifier) ? "NULL" : $"'{item.AlternateIdentifier}'")})";

                var parameters = new List<BigQueryParameter>();

                _client.ExecuteQuery(query, parameters);
            }
        }

        public override async Task UpdateAsync(Segment item)
        {
            var oldRow = await LookupAsync(item.Id);
            if (oldRow != null)
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"UPDATE `{_datasetId}.{_tableId}` SET ");

                queryBuilder.Append($"UdotRouteNumber = {item.UdotRouteNumber}, ");
                queryBuilder.Append($"StartMilePoint = {item.StartMilePoint}, ");
                queryBuilder.Append($"EndMilePoint = {item.EndMilePoint}, ");

                if (!string.IsNullOrEmpty(item.FunctionalType))
                {
                    queryBuilder.Append($"FunctionalType = '{item.FunctionalType}', ");
                }

                if (!string.IsNullOrEmpty(item.Name))
                {
                    queryBuilder.Append($"Name = '{item.Name}', ");
                }

                if (!string.IsNullOrEmpty(item.Direction))
                {
                    queryBuilder.Append($"Direction = '{item.Direction}', ");
                }

                queryBuilder.Append($"SpeedLimit = {item.SpeedLimit}, ");

                if (!string.IsNullOrEmpty(item.Region))
                {
                    queryBuilder.Append($"Region = '{item.Region}', ");
                }

                if (!string.IsNullOrEmpty(item.City))
                {
                    queryBuilder.Append($"City = '{item.City}', ");
                }

                if (!string.IsNullOrEmpty(item.County))
                {
                    queryBuilder.Append($"County = '{item.County}', ");
                }

                queryBuilder.Append($"Shape = '{item.Shape.AsText()}', ");
                queryBuilder.Append($"ShapeWKT = '{item.ShapeWKT}', ");

                if (!string.IsNullOrEmpty(item.AlternateIdentifier))
                {
                    queryBuilder.Append($"AlternateIdentifier = '{item.AlternateIdentifier}', ");
                }

                // Remove the last comma and space
                queryBuilder.Length -= 2;

                queryBuilder.Append($" WHERE Id = @key");

                var query = queryBuilder.ToString();

                var parameters = new List<BigQueryParameter>
        {
            new BigQueryParameter("@key", BigQueryDbType.String, item.Id.ToString())
        };

                await _client.ExecuteQueryAsync(query, parameters);
            }
            else
            {
                var query = $"INSERT INTO `{_datasetId}.{_tableId}` " +
                    $"(Id, UdotRouteNumber, StartMilePoint, EndMilePoint, FunctionalType, Name, Direction, SpeedLimit, Region, City, County, Shape, ShapeWKT, AlternateIdentifier) " +
                    $"VALUES (" +
                    $"{item.Id}, " +
                    $"{item.UdotRouteNumber}, " +
                    $"{item.StartMilePoint}, " +
                    $"{item.EndMilePoint}, " +
                    $"'{item.FunctionalType}', " +
                    $"'{item.Name}', " +
                    $"'{item.Direction}', " +
                    $"{item.SpeedLimit}, " +
                    $"'{item.Region}', " +
                    $"'{item.City}', " +
                    $"'{item.County}', " +
                    $"'{item.Shape.AsText()}', " +
                    $"'{item.ShapeWKT}', " +
                    $"{(string.IsNullOrEmpty(item.AlternateIdentifier) ? "NULL" : $"'{item.AlternateIdentifier}'")})";

                var parameters = new List<BigQueryParameter>();

                await _client.ExecuteQueryAsync(query, parameters);
            }
        }

        public override void UpdateRange(IEnumerable<Segment> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public override async Task UpdateRangeAsync(IEnumerable<Segment> items)
        {
            foreach (var item in items)
            {
                await UpdateAsync(item);
            }
        }

        protected override Segment MapRowToEntity(BigQueryRow row)
        {
            var reader = new WKTReader();
            var wkt = row["Shape"].ToString();
            Geometry shape = wkt != null ? reader.Read(wkt) : null;
            var id = Guid.Parse(row["Id"].ToString());
            return new Segment()
            {
                Id = id,
                UdotRouteNumber = row["UdotRouteNumber"].ToString(),
                StartMilePoint = (double)row["StartMilePoint"],
                EndMilePoint = (double)row["EndMilePoint"],
                FunctionalType = row["FunctionalType"].ToString(),
                Name = row["Name"].ToString(),
                Direction = row["Direction"]?.ToString(),
                SpeedLimit = (long)row["SpeedLimit"],
                Region = row["Region"]?.ToString(),
                City = row["City"]?.ToString(),
                County = row["County"]?.ToString(),
                Shape = shape,
                ShapeWKT = row["ShapeWKT"]?.ToString(),
                AlternateIdentifier = row["AlternateIdentifier"]?.ToString(),
                AccessCategory = row["AccessCategory"]?.ToString(),
                Offset = (long)row["Offset"]
            };
        }

        public override IQueryable<Segment> GetList()
        {
            var query = $"SELECT * FROM `{_datasetId}.{_tableId}`";
            var parameters = new List<BigQueryParameter>();

            var result = _client.ExecuteQuery(query, parameters).ToList();

            // Map the result to a list of ImpactType objects
            return result.Select(row => MapRowToEntity(row)).ToList().AsQueryable();
        }

        public List<Segment> AllSegmentsWithEntity()
        {
            {
                string query = $@"
                SELECT 
                    s.Id,
                    s.UdotRouteNumber,
                    s.StartMilePoint,
                    s.EndMilePoint,
                    s.FunctionalType,
                    s.Name,
                    s.Direction,
                    s.SpeedLimit,
                    s.Region,
                    s.City,
                    s.County,
                    s.Shape,
                    s.ShapeWKT,
                    s.AlternateIdentifier,
                    s.AccessCategory,
                    s.Offset,
                    se.EntityId,
                    se.SourceId,
                    se.SegmentId
                FROM 
                    `{_datasetId}.{_tableId}` s
                LEFT JOIN 
                    `{_datasetId}.segment_entity` se 
                ON 
                    s.Id = se.SegmentId
                ORDER BY 
                    s.Id, se.EntityId;";

                var parameters = new List<BigQueryParameter>();
                var result = _client.ExecuteQuery(query, parameters).ToList();

                var segments = new Dictionary<Guid, Segment>();

                foreach (var row in result)
                {
                    var segmentId = Guid.Parse(row["Id"].ToString());

                    if (!segments.TryGetValue(segmentId, out var segment))
                    {
                        var reader = new WKTReader();
                        var wkt = row["Shape"].ToString();
                        Geometry shape = wkt != null ? reader.Read(wkt) : null;

                        segment = new Segment
                        {
                            Id = segmentId,
                            UdotRouteNumber = row["UdotRouteNumber"].ToString(),
                            StartMilePoint = (double)row["StartMilePoint"],
                            EndMilePoint = (double)row["EndMilePoint"],
                            FunctionalType = row["FunctionalType"].ToString(),
                            Name = row["Name"].ToString(),
                            Direction = row["Direction"]?.ToString(),
                            SpeedLimit = (long)row["SpeedLimit"],
                            Region = row["Region"]?.ToString(),
                            City = row["City"]?.ToString(),
                            County = row["County"]?.ToString(),
                            Shape = shape,
                            ShapeWKT = row["ShapeWKT"]?.ToString(),
                            AlternateIdentifier = row["AlternateIdentifier"]?.ToString(),
                            AccessCategory = row["AccessCategory"]?.ToString(),
                            Offset = (long?)row["Offset"],
                            RouteEntities = new List<SegmentEntity>()
                        };

                        segments.Add(segmentId, segment);
                    }

                    if (row["EntityId"] != null && row["SourceId"] != null && row["SegmentId"] != null)
                    {
                        segment.RouteEntities.Add(new SegmentEntity
                        {
                            EntityId = Convert.ToInt32((long)row["EntityId"]),
                            SourceId = Convert.ToInt32((long)row["SourceId"]),
                            SegmentId = Guid.Parse(row["SegmentId"].ToString())
                        });
                    }
                }

                return segments.Values.ToList();
            }
        }


    }
}
