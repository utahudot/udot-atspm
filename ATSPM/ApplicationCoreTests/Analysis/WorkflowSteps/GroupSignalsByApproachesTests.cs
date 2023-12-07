using ApplicationCoreTests.Fixtures;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class GroupSignalsByApproachesTests : IClassFixture<TestSignalFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Signal _testSignal;

        public GroupSignalsByApproachesTests(ITestOutputHelper output, TestSignalFixture testSignal)
        {
            _output = output;
            _testSignal = testSignal.TestSignal;
        }

        /// <summary>
        /// Tests that it's cancelling correctly, should return a <see cref="TaskCanceledException"/>
        /// </summary>
        [Fact]
        [Trait(nameof(GroupSignalsByApproaches), "Cancellation")]
        public async void GroupSignalsByApproachesTestCancellation()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testSignal, testLogs);

            var sut = new GroupSignalsByApproaches();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(testData, source.Token));
        }

        /// <summary>
        /// Tests the correct number of approaches are extracted from the test signal
        /// </summary>
        [Fact]
        [Trait(nameof(GroupSignalsByApproaches), "Approaches")]
        public async void GroupSignalsByApproachesTestApproaches()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testSignal, testLogs);

            var sut = new GroupSignalsByApproaches();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testSignal.Approaches.Select(s => Tuple.Create(s, testLogs));

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests that the correct sort order of the events has been applied
        /// </summary>
        [Fact]
        [Trait(nameof(GroupSignalsByApproaches), "Sort Order")]
        public async void GroupSignalsByApproachesTestSortOrder()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testSignal, testLogs);

            var sut = new GroupSignalsByApproaches();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testSignal.Approaches.Select(s => Tuple.Create(s, testLogs.OrderBy(o => o.Timestamp).AsEnumerable()));

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests that only events with a SignalIdentifier matching the test signal are forwarded
        /// </summary>
        [Fact]
        [Trait(nameof(GroupSignalsByApproaches), "Signal")]
        public async void GroupSignalsByApproachesTestSignal()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testSignal, testLogs);

            var sut = new GroupSignalsByApproaches();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testSignal.Approaches.Select(s => Tuple.Create(s, testLogs.Where(w => w.SignalIdentifier == _testSignal.SignalIdentifier)));

            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
