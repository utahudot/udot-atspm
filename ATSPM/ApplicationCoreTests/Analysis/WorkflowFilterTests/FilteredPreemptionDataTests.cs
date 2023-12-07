using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredPreemptionDataTests : WorkflowFilterTestsBase
    {
        public FilteredPreemptionDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)DataLoggerEnum.PreemptCallInputOn);
            filteredList.Add((int)DataLoggerEnum.PreemptGateDownInputReceived);
            filteredList.Add((int)DataLoggerEnum.PreemptCallInputOff);
            filteredList.Add((int)DataLoggerEnum.PreemptEntryStarted);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginTrackClearance);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginDwellService);
            filteredList.Add((int)DataLoggerEnum.PreemptionMaxPresenceExceeded);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginExitInterval);

            sut = new FilteredPreemptionData();
        }
    }
}
