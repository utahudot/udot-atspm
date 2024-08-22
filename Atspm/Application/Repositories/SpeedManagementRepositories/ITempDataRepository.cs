using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface ITempDataRepository : IAsyncRepository<TempData>
    {
        public Task<List<TempData>> GetHourlyAggregatedDataForAllSegments();
    }
}
