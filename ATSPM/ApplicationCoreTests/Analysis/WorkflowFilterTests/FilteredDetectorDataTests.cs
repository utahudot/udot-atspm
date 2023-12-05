using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredDetectorDataTests : WorkflowFilterTestsBase
    {
        public FilteredDetectorDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)DataLoggerEnum.DetectorOff);
            filteredList.Add((int)DataLoggerEnum.DetectorOn);

            sut = new FilteredDetectorData();
        }
    }
}
