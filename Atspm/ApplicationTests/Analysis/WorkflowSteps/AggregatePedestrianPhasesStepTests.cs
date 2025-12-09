#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/AggregatePedestrianPhasesStepTests.cs
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
using Utah.Udot.Atspm.Analysis.PedestrianDelay;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class AggregatePedestrianPhasesStepTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public AggregatePedestrianPhasesStepTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        [Fact]
        [Trait(nameof(AggregatePedestrianPhasesStep), "Cancellation")]
        public async Task AggregatePedestrianPhasesTestsCancellation()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            var testData = Tuple.Create(_testLocation, (IEnumerable<IndianaEvent>)[]);

            var sut = new AggregatePedestrianPhasesStep(null);

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(testData, source.Token));
        }

        #region IdentityPedCycles

        [Theory]
        [Trait(nameof(IndianaEventExtensions), "IdentifyPedCycles")]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyPedCyclesSequenceOneTest(bool isPedPhaseOverlap)
        {
            var approach = new Approach()
            {
                ProtectedPhaseNumber = 2,
                IsPedestrianPhaseOverlap = isPedPhaseOverlap
            };

            var location = new Location()
            {
                LocationIdentifier = "1001",
            };

            location.Approaches.Add(approach);

            var startTime = DateTime.Now;
            var endTime = startTime.AddSeconds(4);
            var pedDetectorOnTime = startTime.AddSeconds(2);
            var beginWalkTime = startTime;

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = pedDetectorOnTime, EventCode = (short)IndianaEnumerations.PedDetectorOn, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = startTime, EventCode = (short)IndianaEnumerations.PedestrianBeginWalk, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = endTime, EventCode = (short)IndianaEnumerations.PedestrianBeginChangeInterval, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = startTime, EventCode = (short)IndianaEnumerations.PedestrianOverlapBeginWalk, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = endTime, EventCode = (short)IndianaEnumerations.PedestrianOverlapBeginClearance, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
            };


            var actual = events.IdentifyPedDelayCycles(approach.IsPedestrianPhaseOverlap)[0];

            _output.WriteLine($"{actual}");

            var expected = new PedDelayCycle()
            {
                PedDetectorOn = pedDetectorOnTime,
                Start = startTime,
                BeginWalk = beginWalkTime,
                End = endTime
            };

            _output.WriteLine($"{expected}");

            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.PedDetectorOn, actual.PedDetectorOn);
            Assert.Equal(expected.BeginWalk, actual.BeginWalk);
            Assert.Equal(expected.End, actual.End);
            Assert.Equal(expected.PedDelay, actual.PedDelay);
        }

        [Theory]
        [Trait(nameof(IndianaEventExtensions), "IdentifyPedCycles")]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyPedCyclesSequenceTwoTest(bool isPedPhaseOverlap)
        {
            var approach = new Approach()
            {
                ProtectedPhaseNumber = 2,
                IsPedestrianPhaseOverlap = isPedPhaseOverlap
            };

            var location = new Location()
            {
                LocationIdentifier = "1001",
            };

            location.Approaches.Add(approach);

            var startTime = DateTime.Now;
            var endTime = startTime.AddSeconds(4);
            var pedDetectorOnTime = startTime.AddSeconds(2);
            var beginWalkTime = endTime;

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = pedDetectorOnTime, EventCode = (short)IndianaEnumerations.PedDetectorOn, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = endTime, EventCode = (short)IndianaEnumerations.PedestrianBeginWalk, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = startTime, EventCode = (short)IndianaEnumerations.PedestrianBeginChangeInterval, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = endTime, EventCode = (short)IndianaEnumerations.PedestrianOverlapBeginWalk, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = startTime, EventCode = (short)IndianaEnumerations.PedestrianOverlapBeginClearance, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
            };


            var actual = events.IdentifyPedDelayCycles(approach.IsPedestrianPhaseOverlap)[0];

            _output.WriteLine($"{actual}");

            var expected = new PedDelayCycle()
            {
                PedDetectorOn = pedDetectorOnTime,
                Start = startTime,
                BeginWalk = beginWalkTime,
                End = endTime
            };

            _output.WriteLine($"{expected}");

            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.PedDetectorOn, actual.PedDetectorOn);
            Assert.Equal(expected.BeginWalk, actual.BeginWalk);
            Assert.Equal(expected.End, actual.End);
            Assert.Equal(expected.PedDelay, actual.PedDelay);
        }

        [Theory]
        [Trait(nameof(IndianaEventExtensions), "IdentifyPedCycles")]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyPedCyclesSequenceThreeTest(bool isPedPhaseOverlap)
        {
            var approach = new Approach()
            {
                ProtectedPhaseNumber = 2,
                IsPedestrianPhaseOverlap = isPedPhaseOverlap
            };

            var location = new Location()
            {
                LocationIdentifier = "1001",
            };

            location.Approaches.Add(approach);

            var startTime = DateTime.Now;
            var endTime = startTime.AddSeconds(4);
            var pedDetectorOnTime = startTime.AddSeconds(2);
            var beginWalkTime = endTime;

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = pedDetectorOnTime, EventCode = (short)IndianaEnumerations.PedDetectorOn, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = startTime, EventCode = (short)IndianaEnumerations.PedestrianBeginWalk, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = endTime, EventCode = (short)IndianaEnumerations.PedestrianBeginWalk, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = startTime, EventCode = (short)IndianaEnumerations.PedestrianOverlapBeginWalk, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
                new() { LocationIdentifier = location.LocationIdentifier, Timestamp = endTime, EventCode = (short)IndianaEnumerations.PedestrianOverlapBeginWalk, EventParam = Convert.ToInt16(approach.ProtectedPhaseNumber)},
            };


            var actual = events.IdentifyPedDelayCycles(approach.IsPedestrianPhaseOverlap)[0];

            _output.WriteLine($"{actual}");

            var expected = new PedDelayCycle()
            {
                PedDetectorOn = pedDetectorOnTime,
                Start = startTime,
                BeginWalk = beginWalkTime,
                End = endTime
            };

            _output.WriteLine($"{expected}");

            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.PedDetectorOn, actual.PedDetectorOn);
            Assert.Equal(expected.BeginWalk, actual.BeginWalk);
            Assert.Equal(expected.End, actual.End);
            Assert.Equal(expected.PedDelay, actual.PedDelay);
        }

        #endregion

        #region PedRequests

        //[Fact]
        //[Trait(nameof(IndianaEventExtensions), "PedRequests")]
        //public void PedRequestsTest()
        //{
        //    var startTime = DateTime.Now.Date;

        //    var events = Enumerable.Range(0, 10).Select(s =>
        //    {
        //        var offset = (s % 2 == 0) ? startTime.AddMinutes(s) : startTime.AddHours(s);

        //        return new IndianaEvent()
        //        {
        //            EventCode = (short)IndianaEnumerations.PedDetectorOn,
        //            Timestamp = offset
        //        };
        //    });

        //    foreach (var e in events.OrderBy(o => o.Timestamp))
        //    {
        //        _output.WriteLine($"{e}");
        //    }

        //    var range = new StartEndRange()
        //    {
        //        Start = startTime,
        //        End = startTime.AddMinutes(15)
        //    };

        //    var actual = events.PedRequests(range);

        //    _output.WriteLine($"{actual}");

        //    var expected = 5;

        //    _output.WriteLine($"{expected}");

        //    Assert.Equal(expected, actual);
        //}

        #endregion

        #region CountUniquePedDetections

        [Fact]
        [Trait(nameof(IndianaEventExtensions), "CountUniquePedDetections")]
        public void CountUniquePedDetectionsTest()
        {
            var startTime = DateTime.Now.Date;

            var events = Enumerable.Range(1, 10).Select(s =>
            {
                var offset = (s % 2 == 0) ? s * 10 : s * 20;

                return new IndianaEvent()
                {
                    EventCode = (short)IndianaEnumerations.PedDetectorOn,
                    Timestamp = startTime.AddSeconds(offset)
                };
            });

            var range = new StartEndRange()
            {
                Start = startTime,
                End = startTime.AddMinutes(15)
            };

            var actual = events.CountUniquePedDetections(range);

            _output.WriteLine($"{actual}");

            var expected = 6;

            _output.WriteLine($"{expected}");

            Assert.Equal(expected, actual);
        }

        #endregion










        [Fact(Skip = "Used to create test data")]
        public async Task Stuff()
        {
            {
                //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\Location7115TestData.json").FullName);
                //var Location = JsonConvert.DeserializeObject<Location>(json);

                var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\Pedaggraw.csv");

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

                logs = logs
                    .Where(w => w.EventCode == 0 || w.EventCode == 21 || w.EventCode == 22 || w.EventCode == 90 || w.EventCode == 45 || w.EventCode == 67 || w.EventCode == 68)
                    .Where(w => w.EventParam == 2)
                    .OrderBy(o => o.Timestamp)
                    .ToList();

                _testLocation.Approaches = _testLocation.Approaches.Where(w => w.ProtectedPhaseNumber == 2).ToList();

                var file2 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\pedaggresultdata.csv");

                var output = File.ReadAllLines(file2.FullName)
                       .Skip(1)
                       .Select(x => x.Split(','))
                       .Select(x => new PhasePedAggregation
                       {
                           LocationIdentifier = x[0],
                           PhaseNumber = int.Parse(x[1]),
                           Start = DateTime.Parse(x[2]),
                           End = DateTime.Parse(x[2]).AddMinutes(15),
                           PedCycles = int.Parse(x[3]),
                           PedDelay = double.Parse(x[4]),
                           MinPedDelay = double.Parse(x[5]),
                           MaxPedDelay = double.Parse(x[6]),
                           PedRequests = int.Parse(x[7]),
                           ImputedPedCallsRegistered = int.Parse(x[8]),
                           UniquePedDetections = int.Parse(x[9]),
                           PedBeginWalkCount = int.Parse(x[10]),
                           PedCallsRegisteredCount = int.Parse(x[11])

                       }).ToList();

                _output.WriteLine($"{output.Count}");

                foreach (var o in output)
                {
                    _output.WriteLine($"{o}");
                }

                var result = new AggregatePedestrianPhasesTestData()
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
                File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\AggregatePedestrianPhasesTestData1.json", test);
            }
        }








        [Theory]
        [AnalysisTestData<AggregatePedestrianPhasesTestData>]
        [Trait(nameof(AggregatePedestrianPhasesStep), "From File")]
        public async Task AggregatePedestrianPhasesFromFileTest(Location config, IEnumerable<IndianaEvent> input, IEnumerable<PhasePedAggregation> output)
        {
            var testData = Tuple.Create(config, input);

            var aggDate = input
                .GroupBy(dt => dt.Timestamp)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault().Key;

            var tl = aggDate.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));

            var sut = new AggregatePedestrianPhasesStep(tl);

            var actual = await sut.ExecuteAsync(testData);

            _output.WriteLine($"actual: {actual.Count()}");

            var expected = output;

            _output.WriteLine($"expected: {expected.Count()}");

            Assert.Equivalent(actual, expected);
        }

        public void Dispose()
        {
        }
    }
}
