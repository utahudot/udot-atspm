using System.Collections.Generic;
using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowFilterTests;
using Utah.Udot.Atspm.Data.Enums;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.ApplicationTests.Analysis.WorkflowFilterTests
{
    public class FilterDetectorDataProcessStepTests : WorkflowFilterTestsBase
    {
        public FilterDetectorDataProcessStepTests(ITestOutputHelper output) : base(output)
        {
            filteredCodes =
            [
                (short)IndianaEnumerations.VehicleDetectorOff,
                (short)IndianaEnumerations.VehicleDetectorOn,
            ];

            sut = new FilterDetectorDataProcessStep();
        }
    }
}
