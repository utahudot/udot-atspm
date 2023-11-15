using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using System.Collections.Generic;

namespace ApplicationCoreTests.Analysis.TestObjects
{
    public class RedToRedCyclesTestData
    {
        public List<ControllerEventLog> EventLogs { get; set; }
        public List<RedToRedCycle> RedCycles { get; set; }
    }
}
