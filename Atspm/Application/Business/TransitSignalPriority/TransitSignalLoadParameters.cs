using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class TransitSignalLoadParameters
    {
        public LocationPhases LocationPhases { get; set; }
        public Location Location { get; set; }
        public List<DateTime> Dates { get; set; }

    }
}
