using System.Collections.Generic;
using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowFilterTests;
using Utah.Udot.Atspm.Data.Enums;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.ApplicationTests.Analysis.WorkflowFilterTests
{
    public class FilterPedDataProcessStepTests : WorkflowFilterTestsBase
    {
        public FilterPedDataProcessStepTests(ITestOutputHelper output) : base(output)
        {
            filteredCodes =
            [
                (short)IndianaEnumerations.PhaseOn,
                (short)IndianaEnumerations.PedestrianBeginWalk,
                (short)IndianaEnumerations.PedestrianBeginChangeInterval,
                (short)IndianaEnumerations.PedestrianOverlapBeginWalk,
                (short)IndianaEnumerations.PedestrianOverlapBeginClearance,
                (short)IndianaEnumerations.PedDetectorOn,
                (short)IndianaEnumerations.PedestrianCallRegistered
            ];

            sut = new FilterPedDataProcessStep();
        }
    }
}
