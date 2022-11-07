using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace ControllerEventLogExportUtility
{
    public class ExtractConsoleConfiguration
    {
        public IEnumerable<DateTime> Dates { get; set; }
        public IEnumerable<string> Included { get; set; }
        public IEnumerable<string> Excluded { get; set; }
        public IEnumerable<int> ControllerTypes { get; set; }
        public DirectoryInfo Path { get; set; }
    }
}
