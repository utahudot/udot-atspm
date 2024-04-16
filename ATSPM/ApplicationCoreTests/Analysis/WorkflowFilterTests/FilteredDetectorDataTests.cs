using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredDetectorDataTests : WorkflowFilterTestsBase
    {
        public FilteredDetectorDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)IndianaEnumerations.VehicleDetectorOff);
            filteredList.Add((int)IndianaEnumerations.VehicleDetectorOn);

            sut = new FilteredDetectorData();
        }
    }
}
