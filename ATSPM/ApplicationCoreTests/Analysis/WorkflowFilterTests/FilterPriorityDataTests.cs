using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilterPriorityDataTests : WorkflowFilterTestsBase
    {
        public FilterPriorityDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)IndianaEnumerations.TSPCheckIn);
            filteredList.Add((int)IndianaEnumerations.TSPAdjustmenttoEarlyGreen);
            filteredList.Add((int)IndianaEnumerations.TSPAdjustmenttoExtendGreen);

            sut = new FilterPriorityData();
        }
    }
}
