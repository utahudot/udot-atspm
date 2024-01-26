using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.Watchdog
{
    public class WatchDogResult:BaseResult
    {
        public List <WatchDogLogEventDTO> LogEvents { get; set; }
        
    }
}
