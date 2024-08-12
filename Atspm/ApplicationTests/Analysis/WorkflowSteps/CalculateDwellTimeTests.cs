﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - %Namespace%/CalculateDwellTimeTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using ApplicationCoreTests.Analysis.TestObjects;
using ApplicationCoreTests.Fixtures;
using Utah.Udot.Atspm.Analysis.PreemptionDetails;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Models;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class CalculateDwellTimeTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;
        private const int param = 1;

        public CalculateDwellTimeTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Data Check")]
        public async void CalculateDwellTimeTestsDataCheck()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testLocation, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var expected = new DwellTimeValue()
            {
                locationIdentifier = _testLocation.locationIdentifier,
                PreemptNumber = param,
                Seconds = TimeSpan.FromSeconds(42),
                Start = DateTime.Parse("2023-04-17T00:02:25.5"),
                End = DateTime.Parse("2023-04-17T00:03:07.5")
            };

            Assert.Equivalent(_testLocation, result.Item1);
            Assert.Equivalent(expected, result.Item2.First());
            Assert.Equivalent(param, result.Item3);
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Sort Order")]
        public async void CalculateDwellTimeTestsSortOrder()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testLocation, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var expected = new DwellTimeValue()
            {
                locationIdentifier = _testLocation.locationIdentifier,
                PreemptNumber = param,
                Seconds = TimeSpan.FromSeconds(42),
                Start = DateTime.Parse("2023-04-17T00:02:25.5"),
                End = DateTime.Parse("2023-04-17T00:03:07.5")
            };

            Assert.Equivalent(_testLocation, result.Item1);
            Assert.Equivalent(expected, result.Item2.First());
            Assert.Equivalent(param, result.Item3);
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Preempt Number Filter")]
        public async void CalculateDwellTimeTestsPreemptNumberFilter()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:02:14.5"), EventCode = 102, EventParam = 2},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:02:20.5"), EventCode = 105, EventParam = 2},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:02:25.5"), EventCode = 107, EventParam = 2},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:03:01.3"), EventCode = 104, EventParam = 2},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:03:07.5"), EventCode = 111, EventParam = 2},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testLocation, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Item2.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Location Filter")]
        public async void CalculateDwellTimeTestsLocationFilter()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
                new IndianaEvent() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:02:14.5"), EventCode = 102, EventParam = param},
                new IndianaEvent() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:02:20.5"), EventCode = 105, EventParam = param},
                new IndianaEvent() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:02:25.5"), EventCode = 107, EventParam = param},
                new IndianaEvent() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:03:01.3"), EventCode = 104, EventParam = param},
                new IndianaEvent() { locationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testLocation, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Item2.Count() == 1;

            Assert.True(condition);
        }

        public void Dispose()
        {
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Missing 107 Event")]
        public async void CalculateDwellTimeTestsMissing107Event()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                //new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testLocation, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Item2?.Count() == 0;

            _output.WriteLine($"condition: {condition}");

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Missing 111 Event")]
        public async void CalculateDwellTimeTestsMissing111Event()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                //new IndianaEvent() { locationIdentifier = _testLocation.locationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testLocation, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Item2?.Count() == 0;

            _output.WriteLine($"condition: {condition}");

            Assert.True(condition);
        }

        [Theory]
        [InlineData(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculateDwellTimeTestData1.json")]
        [Trait(nameof(CalculateDwellTime), "From File")]
        public async void CalculateDwellTimeFromFileTest(string file)
        {
            var json = File.ReadAllText(new FileInfo(file).FullName);
            var testFile = JsonConvert.DeserializeObject<PreemptiveProcessTestData>(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            _output.WriteLine($"Configuration: {testFile.Configuration}");
            _output.WriteLine($"Input: {testFile.Input.Count}");
            _output.WriteLine($"Output: {testFile.Output.Count}");

            var testData = Tuple.Create<Location, IEnumerable<IndianaEvent>, int>(testFile.Configuration, testFile.Input, 1);

            var sut = new CalculateDwellTime();

            var result = await sut.ExecuteAsync(testData);

            var expected = testFile.Output;
            var actual = result.Item2;

            Assert.Equivalent(expected, actual);
        }
    }
}
