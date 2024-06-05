using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Domain.Services;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementAggregationRepositories
{
    public interface IHourlySpeedRepository : IAsyncRepository<HourlySpeed>
    {
        public Task AddHourlySpeedAsync(HourlySpeed hourlySpeed);
        #region ExtensionMethods



        #endregion

        #region Obsolete


        #endregion
    }
}
