using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowFilterTests;
using Utah.Udot.Atspm.Data.Enums;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.ApplicationTests.Analysis.WorkflowFilterTests
{
    public class FilterPhaseIntervalChangeDateProcessStepTests : WorkflowFilterTestsBase
    {
        public FilterPhaseIntervalChangeDateProcessStepTests(ITestOutputHelper output) : base(output)
        {
            filteredCodes =
            [
                (short)IndianaEnumerations.PhaseBeginGreen,
                (short)IndianaEnumerations.PhaseBeginYellowChange,
                (short)IndianaEnumerations.PhaseEndYellowChange,
                (short)IndianaEnumerations.PhaseEndRedClearance,
            ];

            sut = new FilterPhaseIntervalChangeDateProcessStep();
        }
    }
}
