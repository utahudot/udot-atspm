using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotAdjustment
    {
        public LinkPivotAdjustment(int linkNumber, string locationIdentifier, string location, int delta, int adjustment)
        {
            LinkNumber = linkNumber;
            LocationIdentifier = locationIdentifier;
            Location = location;
            Delta = delta;
            Adjustment = adjustment;
        }

        public int LinkNumber { get; set; }
        public string LocationIdentifier { get; set; }
        public string Location { get; set; }
        public int Delta { get; set; }
        public int Adjustment { get; set; }
    }
}
