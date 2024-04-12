using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredTerminationsTests : WorkflowFilterTestsBase
    {
        public FilteredTerminationsTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)4);
            filteredList.Add((int)5);
            filteredList.Add((int)6);
            filteredList.Add((int)IndianaEnumerations.PhaseGreenTermination);

            sut = new FilteredTerminations();
        }
    }
}
