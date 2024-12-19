using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class EventLogAggregateConfiguration
    {
        public string AggregationType { get; set; }
        public IEnumerable<DateTime> Dates { get; set; }
    }
}
