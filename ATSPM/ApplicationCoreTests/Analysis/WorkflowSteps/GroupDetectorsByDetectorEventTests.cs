﻿using ApplicationCoreTests.Fixtures;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
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
    public class GroupDetectorsByDetectorEventTests : IClassFixture<TestApproachFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Approach _testApproach;
        private readonly Signal _testSignal;

        public GroupDetectorsByDetectorEventTests(ITestOutputHelper output, TestApproachFixture testApproach)
        {
            _output = output;
            _testApproach = testApproach.TestApproach;
            _testSignal = _testApproach.Signal;
        }

        /// <summary>
        /// Tests that it's cancelling correctly, should return a <see cref="TaskCanceledException"/>
        /// </summary>
        [Fact]
        [Trait(nameof(GroupDetectorsByDetectorEvent), "Cancellation")]
        public async void GroupDetectorsByDetectorEventTestCancellation()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:14.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 19},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 20},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:25.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 21},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:01.3"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 25},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 26},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:06:01.3"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 27},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 28},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new GroupDetectorsByDetectorEvent();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(testData, source.Token));
        }

        /// <summary>
        /// Tests the correct number of detectors are extracted from the test approach
        /// and makes sure the event params are equals the detector channel
        /// </summary>
        [Fact]
        [Trait(nameof(GroupDetectorsByDetectorEvent), "Detectors")]
        public async void GroupDetectorsByDetectorEventTestDetectors()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:14.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 19},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 20},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:25.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 21},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:01.3"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 25},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 26},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:06:01.3"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 27},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 28},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new GroupDetectorsByDetectorEvent();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testApproach.Detectors.Select(s => Tuple.Create(s, s.DetectorChannel, testLogs.Where(w => w.EventParam == s.DetectorChannel)));

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests that the correct sort order of the events has been applied
        /// </summary>
        [Fact]
        [Trait(nameof(GroupDetectorsByDetectorEvent), "Sort Order")]
        public async void GroupDetectorsByDetectorEventTestSortOrder()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:06:01.3"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:14.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:25.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:01.3"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new GroupDetectorsByDetectorEvent();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testApproach.Detectors.Select(s => Tuple.Create(s, s.DetectorChannel, testLogs.OrderBy(o => o.Timestamp).AsEnumerable()));

            Assert.Equal(expected.Where(w => w.Item2 == 2), actual.Where(w => w.Item2 == 2));
        }

        /// <summary>
        /// Tests that only events with a SignalIdentifier matching the test signal are forwarded
        /// </summary>
        [Fact]
        [Trait(nameof(GroupDetectorsByDetectorEvent), "Signal")]
        public async void GroupDetectorsByDetectorEventTestSignal()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:14.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 19},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 20},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:25.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 21},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:01.3"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 25},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 26},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:06:01.3"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 27},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 28},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)DataLoggerEnum.DetectorOn, EventParam = 2},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new GroupDetectorsByDetectorEvent();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testApproach.Detectors.Select(s => Tuple.Create(s, s.DetectorChannel, testLogs
                .Where(w => w.SignalIdentifier == _testSignal.SignalIdentifier)
                .Where(w => w.EventParam == s.DetectorChannel)));

            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}