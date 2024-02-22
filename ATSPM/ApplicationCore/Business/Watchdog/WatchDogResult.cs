using ATSPM.Application.Business.Common;
using System.Collections.Generic;

namespace ATSPM.Application.Business.Watchdog
{
    public class WatchDogResult : BaseResult
    {
        public List<WatchDogLogEventDTO> LogEvents { get; set; }

    }
}
