using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Models;
using System.Collections.Generic;

namespace ApplicationCoreTests.Analysis.TestObjects
{
    public abstract class AnalysisTestDataBase
    {
        public object Configuration { get; set; }
        public object Input { get; set; }
        public object Output { get; set;}
    }

    public class DetectorEventCountAggregationTestData : AnalysisTestDataBase { }









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
        public List<Approach> Configuration { get; set; }
        public List<Volumes> Input { get; set; }
        public TotalVolumes Output { get; set; }
    }

    public class RedToRedCyclesTestData
    {
        public Approach Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<RedToRedCycle> Output { get; set; }
    }

    public class PreemptiveProcessTestData
    {
        public Location Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<PreempDetailValueBase> Output { get; set; }
    }

    public class AggregatePriorityCodesTestData
    {
        public Location Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<PriorityAggregation> Output { get; set; }
    }

    public class AggregatePreemptCodesTestData
    {
        public Location Configuration { get; set; }
        public List<ControllerEventLog> Input { get; set; }
        public List<PreemptionAggregation> Output { get; set; }
    }
}
