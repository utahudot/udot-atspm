#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/AggregateDetectorEventsTests.cs
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;
using Utah.Udot.NetStandardToolkit.Common;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class AggregateDetectorEventsTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public AggregateDetectorEventsTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        //[Fact]
        //[Trait(nameof(AggregateDetectorEventsStep), "Cancellation")]
        //public async void AggregateDetectorEventsTestsCancellation()
        //{
        //    var source = new CancellationTokenSource();
        //    source.Cancel();

        //    var testLogs = new List<IndianaEvent>
        //    {
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:01.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:02.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //    }.AsEnumerable();

        //    var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

        //    var sut = new AggregateDetectorEventsStep(TimeSpan.FromMinutes(15));

        //    await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(testData, source.Token));
        //}

        ///// <summary>
        ///// Tests that only events with a LocationIdentifier matching the test Location are forwarded
        ///// </summary>
        //[Fact]
        //[Trait(nameof(AggregateDetectorEventsStep), "Location")]
        //public async void AggregateDetectorEventsTestLocation()
        //{
        //    var testLogs = new List<IndianaEvent>
        //    {
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:01.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:02.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:03:03.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:04:04.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //    }.AsEnumerable();

        //    var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

        //    var sut = new AggregateDetectorEventsStep(TimeSpan.FromMinutes(15));

        //    var actual = await sut.ExecuteAsync(testData);

        //    var expected = new DetectorEventCountAggregation()
        //    {
        //        LocationIdentifier = _testLocation.LocationIdentifier,
        //        ApproachId = _testDetector.ApproachId,
        //        DetectorPrimaryId = _testDetector.Id,
        //        EventCount = 2,
        //        Start = DateTime.Parse("4/17/2023 00:00:00.0"),
        //        End = DateTime.Parse("4/17/2023 00:15:00.0")
        //    };

        //    Assert.Collection(actual,
        //        a => Assert.Equivalent(expected, a));
        //}

        ///// <summary>
        ///// Tests that only the event params that match the detector channel are forwarded
        ///// </summary>
        //[Fact]
        //[Trait(nameof(AggregateDetectorEventsStep), "Detector Filter")]
        //public async void AggregateDetectorEventsTestDetectorFilter()
        //{
        //    var testLogs = new List<IndianaEvent>
        //    {
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:01.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:02.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:03.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 100},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:04.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 100},
        //    }.AsEnumerable();

        //    var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

        //    var sut = new AggregateDetectorEventsStep(TimeSpan.FromMinutes(15));

        //    var actual = await sut.ExecuteAsync(testData);

        //    var expected = new DetectorEventCountAggregation()
        //    {
        //        LocationIdentifier = _testLocation.LocationIdentifier,
        //        ApproachId = _testDetector.ApproachId,
        //        DetectorPrimaryId = _testDetector.Id,
        //        EventCount = 2,
        //        Start = DateTime.Parse("4/17/2023 00:00:00.0"),
        //        End = DateTime.Parse("4/17/2023 00:15:00.0")
        //    };

        //    Assert.Collection(actual,
        //        a => Assert.Equivalent(expected, a));
        //}

        ///// <summary>
        ///// Tests that only IndianaEnumerations.VehicleDetectorOn events are forwarded
        ///// </summary>
        //[Fact]
        //[Trait(nameof(AggregateDetectorEventsStep), "Event Code Filter")]
        //public async void AggregateDetectorEventsTestEventCodeFilter()
        //{
        //    var testLogs = Enumerable.Range(1, 256).Select(s => new IndianaEvent()
        //    {
        //        LocationIdentifier = _testLocation.LocationIdentifier,
        //        Timestamp = DateTime.Parse("4/17/2023 00:00:01.0"),
        //        EventCode = Convert.ToInt16(s),
        //        EventParam = Convert.ToInt16(_testDetector.DetectorChannel)
        //    }).ToList().AsEnumerable();

        //    var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

        //    var sut = new AggregateDetectorEventsStep(TimeSpan.FromMinutes(15));

        //    var actual = await sut.ExecuteAsync(testData);

        //    var expected = new DetectorEventCountAggregation()
        //    {
        //        LocationIdentifier = _testLocation.LocationIdentifier,
        //        ApproachId = _testDetector.ApproachId,
        //        DetectorPrimaryId = _testDetector.Id,
        //        EventCount = 1,
        //        Start = DateTime.Parse("4/17/2023 00:00:00.0"),
        //        End = DateTime.Parse("4/17/2023 00:15:00.0")
        //    };

        //    Assert.Collection(actual,
        //        a => Assert.Equivalent(expected, a));
        //}

        ///// <summary>
        ///// Tests if the timestamp of the event is equal to the start of the bin then it should be in that bin
        ///// </summary>
        //[Fact]
        //[Trait(nameof(AggregateDetectorEventsStep), "Bin Start")]
        //public async void AggregateDetectorEventsTestBinStart()
        //{
        //    var testLogs = new List<IndianaEvent>
        //    {
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:00:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:10:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //    }.AsEnumerable();

        //    var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

        //    var sut = new AggregateDetectorEventsStep(TimeSpan.FromMinutes(15));

        //    var actual = await sut.ExecuteAsync(testData);

        //    var expected = new DetectorEventCountAggregation()
        //    {
        //        LocationIdentifier = _testLocation.LocationIdentifier,
        //        ApproachId = _testDetector.ApproachId,
        //        DetectorPrimaryId = _testDetector.Id,
        //        EventCount = 3,
        //        Start = DateTime.Parse("4/17/2023 00:00:00.0"),
        //        End = DateTime.Parse("4/17/2023 00:15:00.0")
        //    };

        //    Assert.Collection(actual,
        //        a => Assert.Equivalent(expected, a));
        //}

        ///// <summary>
        ///// Tests that the event timestamps are organized into the correct bin
        ///// </summary>
        //[Fact]
        //[Trait(nameof(AggregateDetectorEventsStep), "Bin Groups")]
        //public async void AggregateDetectorEventsTestBinGroups()
        //{
        //    var testLogs = new List<IndianaEvent>
        //    {
        //        //group a
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:00:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:10:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},

        //        //group b
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:15:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:20:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //        new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:25:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
        //    }.AsEnumerable();

        //    var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

        //    var sut = new AggregateDetectorEventsStep(TimeSpan.FromMinutes(15));

        //    var actual = await sut.ExecuteAsync(testData);

        //    var expectedA = new DetectorEventCountAggregation()
        //    {
        //        LocationIdentifier = _testLocation.LocationIdentifier,
        //        ApproachId = _testDetector.ApproachId,
        //        DetectorPrimaryId = _testDetector.Id,
        //        EventCount = 3,
        //        Start = DateTime.Parse("4/17/2023 00:00:00.0"),
        //        End = DateTime.Parse("4/17/2023 00:15:00.0")
        //    };

        //    var expectedB = new DetectorEventCountAggregation()
        //    {
        //        LocationIdentifier = _testLocation.LocationIdentifier,
        //        ApproachId = _testDetector.ApproachId,
        //        DetectorPrimaryId = _testDetector.Id,
        //        EventCount = 3,
        //        Start = DateTime.Parse("4/17/2023 00:15:00.0"),
        //        End = DateTime.Parse("4/17/2023 00:30:00.0")
        //    };

        //    Assert.Collection(actual,
        //        a => Assert.Equivalent(expectedA, a),
        //        a => Assert.Equivalent(expectedB, a));
        //}

        [Fact(Skip = "skip")]
        public async Task Stuff()
        {
            {
                //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\Location7115TestData.json").FullName);
                //var Location = JsonConvert.DeserializeObject<Location>(json);

                var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\TempDetectorTestData.csv");

                var logs = File.ReadAllLines(file1.FullName)
                       .Skip(1)
                       .Select(x => x.Split(','))
                       .Select(x => new IndianaEvent
                       {
                           LocationIdentifier = x[0],
                           Timestamp = DateTime.Parse(x[1]),
                           EventCode = short.Parse(x[2]),
                           EventParam = short.Parse(x[3])
                       }).ToList();

                //logs = logs
                //    .Where(w => w.EventCode == 0 || w.EventCode == 21 || w.EventCode == 22 || w.EventCode == 90 || w.EventCode == 45 || w.EventCode == 67 || w.EventCode == 68)
                //    .Where(w => w.EventParam == 2)
                //    .OrderBy(o => o.Timestamp)
                //    .ToList();

                //_testLocation.Approaches = _testLocation.Approaches.Where(w => w.ProtectedPhaseNumber == 2).ToList();

                //var file2 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\pedaggresultdata.csv");

                //var output = File.ReadAllLines(file2.FullName)
                //       .Skip(1)
                //       .Select(x => x.Split(','))
                //       .Select(x => new PhasePedAggregation
                //       {
                //           LocationIdentifier = x[0],
                //           PhaseNumber = int.Parse(x[1]),
                //           Start = DateTime.Parse(x[2]),
                //           End = DateTime.Parse(x[2]).AddMinutes(15),
                //           PedCycles = int.Parse(x[3]),
                //           PedDelay = double.Parse(x[4]),
                //           MinPedDelay = double.Parse(x[5]),
                //           MaxPedDelay = double.Parse(x[6]),
                //           PedRequests = int.Parse(x[7]),
                //           ImputedPedCallsRegistered = int.Parse(x[8]),
                //           UniquePedDetections = int.Parse(x[9]),
                //           PedBeginWalkCount = int.Parse(x[10]),
                //           PedCallsRegisteredCount = int.Parse(x[11])

                //       }).ToList();

                //_output.WriteLine($"{output.Count}");

                //foreach (var o in output)
                //{
                //    _output.WriteLine($"{o}");
                //}

                var output = new List<DetectorEventCountAggregation>();

                var result = new AggregateDetectorEventCountTestData()
                {
                    Configuration = _testLocation,
                    Input = logs,
                    Output = output
                };


                var test = JsonConvert.SerializeObject(result, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                });
                File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\AggregateDetectorEventCountTestData1.json", test);
            }
        }

        [Theory]
        [AnalysisTestData<AggregateDetectorEventCountTestData>]
        [Trait(nameof(AggregateDetectorEventsStep), "From File")]
        public async Task AggregatePedestrianPhasesFromFileTest(Location config, IEnumerable<IndianaEvent> input, IEnumerable<DetectorEventCountAggregation> output)
        {
            var testData = Tuple.Create(config, input);

            var aggDate = input
                .GroupBy(dt => dt.Timestamp)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault().Key;

            var tl = aggDate.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));

            var sut = new AggregateDetectorEventsStep(tl);

            var actual = await sut.ExecuteAsync(testData);

            _output.WriteLine($"actual count: {actual.Count()}");

            foreach (var a in actual)
            {
                _output.WriteLine($"actual: {a}");
            }

            //_output.WriteLine($"actual: {actual.Count()}");

            //var expected = output;

            //_output.WriteLine($"expected: {expected.Count()}");

            //Assert.Equivalent(actual, expected);
        }

        public void Dispose()
        {
        }
    }
}
