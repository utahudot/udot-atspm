using ATSPM.Data.Models.SpeedManagementConfigModels;
using ATSPM.Domain.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface ITempDataRepository : IAsyncRepository<TempData>
    {
        public Task<List<TempData>> GetHourlyAggregatedDataForAllSegments();
    }
}
