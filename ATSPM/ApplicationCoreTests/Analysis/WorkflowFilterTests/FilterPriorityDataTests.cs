using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilterPriorityDataTests : WorkflowFilterTestsBase
    {
        public FilterPriorityDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)112);
            filteredList.Add((int)113);
            filteredList.Add((int)114);

            sut = new FilterPriorityData();
        }
    }
}
