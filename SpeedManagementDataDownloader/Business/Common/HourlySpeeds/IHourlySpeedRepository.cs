using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Common.HourlySpeeds
{
    public interface IHourlySpeedRepository
    {
        Task AddHourlySpeedsAsync(List<HourlySpeed> hourlySpeeds);
    }
}
