using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public abstract class Aggregation
    {
        public abstract DateTime BinStartTime { get; set; }
    }
}
