using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredPlanDataTests : WorkflowFilterTestsBase
    {
        public FilteredPlanDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)DataLoggerEnum.CoordPatternChange);

            sut = new FilteredPlanData();
        }
    }
}
