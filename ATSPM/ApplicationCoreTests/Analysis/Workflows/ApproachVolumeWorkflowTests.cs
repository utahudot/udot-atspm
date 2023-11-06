using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;

namespace ApplicationCoreTests.Analysis.Workflows
{
    public class ApproachVolumeWorkflowTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public ApproachVolumeWorkflowTests(ITestOutputHelper output)
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
