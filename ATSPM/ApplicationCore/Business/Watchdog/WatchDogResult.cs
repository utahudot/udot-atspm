using ATSPM.ReportApi.Business.Common;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.Watchdog
{
    public class WatchDogResult : BaseResult
    {
        public List<WatchDogLogEventDTO> LogEvents { get; set; }

    }
}
