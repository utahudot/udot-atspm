using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.Workflows
{
    public class PreemptionDetailsWorkflowTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public PreemptionDetailsWorkflowTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        //[Trait(nameof(AssignCyclesToVehicles), "Arrival on Yellow")]
        public async void NotCompleted()
        {
            Assert.False(true);
        }

        public void Dispose()
        {
        }
    }
}
