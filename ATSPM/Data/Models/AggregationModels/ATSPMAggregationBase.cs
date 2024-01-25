using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Models.AggregationModels
{
    public abstract class ATSPMAggregationBase : StartEndRange
    {
        public DateTime BinStartTime { get; set; }
    }
}
