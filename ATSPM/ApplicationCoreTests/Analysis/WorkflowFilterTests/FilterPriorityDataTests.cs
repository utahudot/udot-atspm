using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilterPriorityDataTests : WorkflowFilterTestsBase
    {
        public FilterPriorityDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)DataLoggerEnum.TSPCheckIn);
            filteredList.Add((int)DataLoggerEnum.TSPAdjustmenttoEarlyGreen);
            filteredList.Add((int)DataLoggerEnum.TSPAdjustmenttoExtendGreen);

            sut = new FilterPriorityData();
        }
    }
}
