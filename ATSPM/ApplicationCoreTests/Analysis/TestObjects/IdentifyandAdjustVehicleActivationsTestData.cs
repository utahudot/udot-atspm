using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using System.Collections.Generic;

namespace ApplicationCoreTests.Analysis.TestObjects
{
    public class IdentifyandAdjustVehicleActivationsTestData
    {
        public List<ControllerEventLog> EventLogs { get; set; }
        public List<CorrectedDetectorEvent> CorrectedDetectorEvents { get; set; }
    }
}
