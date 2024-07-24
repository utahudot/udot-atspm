
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using System.Threading.Tasks;

namespace ATSPM.Application.SpeedManagement.Business.CongestionTracking
{
    public interface ICongestionTrackingService
    {
        Task<CongestionTrackingDto> GetReportData(CongestionTrackingOptions options);
    }
}
