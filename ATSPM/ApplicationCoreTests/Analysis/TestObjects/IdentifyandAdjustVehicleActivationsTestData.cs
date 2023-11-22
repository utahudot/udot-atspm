using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using System.Collections.Generic;

namespace ApplicationCoreTests.Analysis.TestObjects
{
    public class IdentifyandAdjustVehicleActivationsTestData
    {
        public Approach Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<CorrectedDetectorEvent> Output { get; set; }
    }

    public class CalculatePhaseVolumeTestData
    {
        public Approach Configuration { get; set; }
        public List<CorrectedDetectorEvent> Input { get; set; }
        public Volumes Output { get; set; }
    }

    public class CalculateTotalVolumeTestData
    {
        public Signal Configuration { get; set; }
        public List<Volumes> Input { get; set; }
        public TotalVolumes Output { get; set; }
    }
}
