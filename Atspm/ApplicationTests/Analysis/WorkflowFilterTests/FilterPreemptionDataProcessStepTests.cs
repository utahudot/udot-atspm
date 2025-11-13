using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowFilterTests;
using Utah.Udot.Atspm.Data.Enums;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.ApplicationTests.Analysis.WorkflowFilterTests
{
    public class FilterPreemptionDataProcessStepTests : WorkflowFilterTestsBase
    {
        public FilterPreemptionDataProcessStepTests(ITestOutputHelper output) : base(output)
        {
            filteredCodes =
            [
                (short)IndianaEnumerations.PreemptCallInputOn,
                (short)IndianaEnumerations.PreemptGateDownInputReceived,
                (short)IndianaEnumerations.PreemptCallInputOff,
                (short)IndianaEnumerations.PreemptEntryStarted,
                (short)IndianaEnumerations.PreemptionBeginTrackClearance,
                (short)IndianaEnumerations.PreemptionBeginDwellService,
                (short)IndianaEnumerations.PreemptionMaxPresenceExceeded,
                (short)IndianaEnumerations.PreemptionBeginExitInterval,
            ];

            sut = new FilterPreemptionDataProcessStep();
        }
    }
}
