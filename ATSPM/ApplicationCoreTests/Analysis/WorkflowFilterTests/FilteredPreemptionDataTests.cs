using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredPreemptionDataTests : WorkflowFilterTestsBase
    {
        public FilteredPreemptionDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)IndianaEnumerations.PreemptCallInputOn);
            filteredList.Add((int)IndianaEnumerations.PreemptGateDownInputReceived);
            filteredList.Add((int)IndianaEnumerations.PreemptCallInputOff);
            filteredList.Add((int)IndianaEnumerations.PreemptEntryStarted);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginTrackClearance);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginDwellService);
            filteredList.Add((int)110);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginExitInterval);

            sut = new FilteredPreemptionData();
        }
    }
}
