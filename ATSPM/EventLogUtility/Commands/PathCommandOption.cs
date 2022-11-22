using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class PathCommandOption : Option<DirectoryInfo>
    {
        public PathCommandOption() : base("--path", () => new DirectoryInfo(Path.GetTempPath()), "Path to directory")
        {
            AddAlias("-p");
        }
    }
}
