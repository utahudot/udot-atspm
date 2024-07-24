using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Models.SpeedManagement.CongestionTracking
{
    public class CongestionTrackingOptions
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int SegmentId { get; set; }

    }
}
