using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using System;
using Google.Cloud.BigQuery.V2;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementAggregationRepositories
{
    ///<inheritdoc cref="IRouteRepository"/>
    public class RouteBQRepository : ATSPMRepositoryBQBase<Route>, IRouteRepository
    {
        private readonly BigQueryClient _client;
        private readonly string _datasetId;
        private readonly string _tableId;
        private readonly ILogger<ATSPMRepositoryBQBase<Route>> _logger;

        public RouteBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<Route>> log) : base(client, datasetId, tableId, log)
        {
            _client = client;
            _datasetId = datasetId;
            _tableId = tableId;
            _logger = log;
        }

        public async Task AddRoutesAsync(IEnumerable<Route> routes)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            List<BigQueryInsertRow> insertRows = new List<BigQueryInsertRow>();
            foreach (var route in routes)
            {
                insertRows.Add(CreateRow(route));
            }
            await table.InsertRowsAsync(insertRows);
        }

        public async Task AddRouteAsync(Route route)
        {
            var table = _client.GetTable(_datasetId, _tableId);
            var insertRow = CreateRow(route);
            await table.InsertRowAsync(insertRow);
        }

        protected override BigQueryInsertRow CreateRow(Route route)
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

        public override IQueryable<Route> GetList()
        {
            throw new NotImplementedException();
        }

        public override Route Lookup(object key)
        {
            throw new NotImplementedException();
        }

        public override Route Lookup(Route item)
        {
            throw new NotImplementedException();
        }

        public override Task<Route> LookupAsync(object key)
        {
            throw new NotImplementedException();
        }

        public override Task<Route> LookupAsync(Route item)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Route item)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveAsync(Route item)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRange(IEnumerable<Route> items)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveRangeAsync(IEnumerable<Route> items)
        {
            throw new NotImplementedException();
        }

        public override void Update(Route item)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateAsync(Route item)
        {
            throw new NotImplementedException();
        }

        public override void UpdateRange(IEnumerable<Route> items)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateRangeAsync(IEnumerable<Route> items)
        {
            throw new NotImplementedException();
        }

        protected override Route MapRowToEntity(BigQueryRow row)
        {
            throw new NotImplementedException();
        }
    }
}
