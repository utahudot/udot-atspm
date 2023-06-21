using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Configuration
{
    public class EventLogLoggingConfiguration
    {
        public IEnumerable<string> Included { get; set; }
        public IEnumerable<string> Excluded { get; set; }
        public IEnumerable<int> ControllerTypes { get; set; }
        public DirectoryInfo Path { get; set; }
    }
}
