using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementAggregationRepositories
{
    public interface IRouteRepository : IAsyncRepository<Route>
    {
        public Task AddRoutesAsync(IEnumerable<Route> routes);
        public Task AddRouteAsync(Route route);
    }
}
