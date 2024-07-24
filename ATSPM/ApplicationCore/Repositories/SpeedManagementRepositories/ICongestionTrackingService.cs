using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface ICongestionTrackingService
    {
        Task<CongestionTrackingDto> GetReportData(CongestionTrackingOptions options);
    }
}
