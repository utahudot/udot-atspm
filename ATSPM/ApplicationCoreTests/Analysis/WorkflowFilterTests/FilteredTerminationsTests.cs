using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredTerminationsTests : WorkflowFilterTestsBase
    {
        public FilteredTerminationsTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)IndianaEnumerations.PhaseGapOut);
            filteredList.Add((int)IndianaEnumerations.PhaseMaxOut);
            filteredList.Add((int)IndianaEnumerations.PhaseForceOff);
            filteredList.Add((int)IndianaEnumerations.PhaseGreenTermination);

            sut = new FilteredTerminations();
        }
    }
}
