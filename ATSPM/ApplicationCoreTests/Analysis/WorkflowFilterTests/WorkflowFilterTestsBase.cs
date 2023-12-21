using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public abstract class WorkflowFilterTestsBase : IDisposable
    {
        protected readonly ITestOutputHelper _output;

        protected List<int> filteredList = new();
        protected FilterEventCodeLocationBase sut;

        protected Tuple<Location, IEnumerable<ControllerEventLog>> testData;

        public WorkflowFilterTestsBase(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CheckFilterPass()
        {
            var testLocation = new Location() { LocationIdentifier = "1001" };
            var testLogs = Enumerable.Range(0, 1000).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = testLocation.LocationIdentifier,
                Timestamp = DateTime.Now.AddSeconds(s),
                EventCode = s,
                EventParam = 1
            }).ToList();

            testData = Tuple.Create(testLocation, testLogs.AsEnumerable());

            sut.Post(testData);
            sut.Complete();

            var actual = sut.Receive();
            var expected = Tuple.Create(testLocation, testLogs.Where(w => filteredList.Contains(w.EventCode)));

            //foreach (var a in actual.Item2)
            //    _output.WriteLine($"actual: {a}");

            //foreach (var e in expected.Item2)
            //    _output.WriteLine($"expected: {e}");

            Assert.Equal(expected, actual);
        }

        public virtual void Dispose()
        {
            //testData = null;
        }
    }
}
