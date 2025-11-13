using System.Collections.Generic;
using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowFilterTests;
using Utah.Udot.Atspm.Data.Enums;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.ApplicationTests.Analysis.WorkflowFilterTests
{
    public class FilterPriorityDataProcessStepTests : WorkflowFilterTestsBase
    {
        public FilterPriorityDataProcessStepTests(ITestOutputHelper output) : base(output)
        {
            filteredCodes =
            [
                (short)IndianaEnumerations.TSPCheckIn,
                (short)IndianaEnumerations.TSPAdjustmenttoEarlyGreen,
                (short)IndianaEnumerations.TSPAdjustmenttoExtendGreen,
            ];

            sut = new FilterPriorityDataProcessStep();
        }
    }
}
