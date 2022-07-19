using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public class Speed_Events
    {
        public string DetectorID { get; set; }
        public int MPH { get; set; }
        public int KPH { get; set; }
        public DateTime timestamp { get; set; }
    }
}
