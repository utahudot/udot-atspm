using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredTerminationsTests : WorkflowFilterTestsBase
    {
        public FilteredTerminationsTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseGapOut);
            filteredList.Add((int)DataLoggerEnum.PhaseMaxOut);
            filteredList.Add((int)DataLoggerEnum.PhaseForceOff);
            filteredList.Add((int)DataLoggerEnum.PhaseGreenTermination);

            sut = new FilteredTerminations();
        }
    }
}
