using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class GeneratePurdueCoordinationResultTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public GeneratePurdueCoordinationResultTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(GeneratePurdueCoordinationResultTests), "DurationCheck")]
        public async void CalculateTimingPlansDurationCheckTest()
        {
            //var sut = new GeneratePurdueCoordinationResult();

            //var testEvents = Enumerable.Range(1, 10).Select(s => new ControllerEventLog()
            //{
            //    SignalId = "1001",
            //    EventCode = (int)DataLoggerEnum.CoordPatternChange,
            //    EventParam = 1,
            //    Timestamp = DateTime.Now.AddSeconds(s)

            //});

            //var result = await sut.ExecuteAsync(testEvents);

            //var condition = result.SelectMany(s => s).All(a => Math.Round((a.End - a.Start).TotalSeconds, 0) == 1);

            Assert.False(true);
        }



        public void Dispose()
        {
        }
    }
}
