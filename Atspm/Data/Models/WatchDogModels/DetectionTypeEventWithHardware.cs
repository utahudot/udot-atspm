using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Data.Models.WatchDogModels
{
    public class DetectionTypeEventWithHardware
    {
        public DetectionTypes DetectionTypeId { get; set; }

        public string DetectionTypeName { get; set; }
        
        public DetectionHardwareTypes DetectionHardware { get; set; }
    }
}
