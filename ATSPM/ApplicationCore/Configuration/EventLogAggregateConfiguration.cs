using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Configuration
{
    public class EventLogAggregateConfiguration
    {
        public string AggregationType { get; set; }
        public int BinSize { get; set; }
        public IEnumerable<DateTime> Dates { get; set; }
        public IEnumerable<string> Included { get; set; }
        public IEnumerable<string> Excluded { get; set; }
    }
}
