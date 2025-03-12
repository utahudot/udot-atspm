using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class LocationPhases
    {
        public string LocationIdentifier { get; set; }
        public List<int> DesignatedPhases { get; set; }
    }
}
