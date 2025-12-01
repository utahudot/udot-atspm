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

using Lextm.SharpSnmpLib;
using MailKit.Search;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class AggregatePhaseCyclesTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public AggregatePhaseCyclesTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        [Fact(Skip = "skip")]
        public async Task Stuff()
        {
            {
                //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\Location7115TestData.json").FullName);
                //var Location = JsonConvert.DeserializeObject<Location>(json);

                var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\TempPhaseCycleTestData.csv");

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

                var output = new List<PhaseCycleAggregation>();

                var result = new AggregatePhaseCycleTestData()
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
                File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\AggregatePhaseCycleTestData1.json", test);
            }
        }

        [Theory]
        [AnalysisTestData<AggregatePhaseCycleTestData>]
        [Trait(nameof(AggregateDetectorEventsStep), "From File")]
        public async Task AggregatePhaseCyclesFromFileTest(Location config, IEnumerable<IndianaEvent> input, IEnumerable<PhaseCycleAggregation> output)
        {
            //var testData = Tuple.Create(config, input);

            var phaseNumber = 2;

            // Pre-filter cycles and intervals for the target phase
            var redCycles = input.IdentifyRedToRedCycles().Where(w => w.LocationIdentifier == "7115" && w.PhaseNumber == phaseNumber).ToList();
            var greenCycles = input.IdentifyGreenToGreenCycles().Where(w => w.LocationIdentifier == "7115" && w.PhaseNumber == phaseNumber).ToList();

            _output.WriteLine($"red cycles: {redCycles.Count}");
            _output.WriteLine($"green cycles: {greenCycles.Count}");

            // Find the most common timestamp (aggregation date)
            var aggDate = input
                .GroupBy(dt => dt.Timestamp)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            // Build interval spans in a single pass
            //var dur = input
            //    .FromSpecification(new IndianaPhaseIntervalChangesDataSpecification())
            //    .Where(w => w.EventParam == phaseNumber)
            //    .KeepFirstSequentialEvent(IndianaEnumerations.PhaseBeginGreen)
            //    .KeepFirstSequentialEvent(IndianaEnumerations.PhaseBeginYellowChange)
            //    .KeepFirstSequentialEvent(IndianaEnumerations.PhaseEndYellowChange)
            //    .SlidingWindow(2)
            //    .Select(chunk =>
            //    {
            //        var codes = chunk.Select(s => s.EventCode).ToArray();
            //        return codes switch
            //        {
            //            [var a, var b] when a == (short)IndianaEnumerations.PhaseBeginGreen && b == (short)IndianaEnumerations.PhaseBeginYellowChange
            //                => (IntervalSpan)new GreenInterval { Start = chunk[0].Timestamp, End = chunk[1].Timestamp },
            //            [var a, var b] when a == (short)IndianaEnumerations.PhaseEndYellowChange && b == (short)IndianaEnumerations.PhaseBeginGreen
            //                => (IntervalSpan)new RedInterval { Start = chunk[0].Timestamp, End = chunk[1].Timestamp },
            //            [var a, var b] when a == (short)IndianaEnumerations.PhaseBeginYellowChange && b == (short)IndianaEnumerations.PhaseEndYellowChange
            //                => (IntervalSpan)new YellowInterval { Start = chunk[0].Timestamp, End = chunk[1].Timestamp },
            //            _ => null,
            //        };
            //    })
            //    .Where(x => x != null)
            //    .ToList();

            // Group cycles by phase number
            var cyclesGroup = redCycles.Concat<CycleBase>(greenCycles)
                .GroupBy(g => (g.LocationIdentifier, g.PhaseNumber))
                .OrderBy(o => o.Key);

            //foreach (var stuff in cyclesGroup.SelectMany(m => m).OrderBy(o => o.Start))
            //{
            //    _output.WriteLine($"{stuff}");
            //}

            foreach (var group in cyclesGroup)
            {
                var tl = aggDate.CreateTimeline<PhaseCycleAggregation>(TimeSpan.FromMinutes(15));

                foreach (var f in tl.Segments)
                {
                    f.LocationIdentifier = group.Key.LocationIdentifier;
                    f.PhaseNumber = group.Key.PhaseNumber;
                    f.TotalRedToRedCycles = group.OfType<RedToRedCycle>().Count(c => f.InRange(c.Start));
                    f.TotalGreenToGreenCycles = group.OfType<GreenToGreenCycle>().Count(c => f.InRange(c.Start));

                    // Fix for CS0029 and CS1662 errors
                    //var compare = ;
                    var r = group.Select(s => s.RedInterval).Distinct(new LambdaEqualityComparer<IntervalSpan>((a, b) => a.Start == b.Start && a.End == b.End)).ToList();
                    var y = group.Select(s => s.YellowInterval).Distinct(new LambdaEqualityComparer<IntervalSpan>((a, b) => a.Start == b.Start && a.End == b.End)).ToList();
                    var g = group.Select(s => s.GreenInterval).Distinct(new LambdaEqualityComparer<IntervalSpan>((a, b) => a.Start == b.Start && a.End == b.End)).ToList();

                    //foreach (var stuff in g.OrderBy(o => o.Start))
                    //{
                    //    _output.WriteLine($"{stuff}");
                    //}


                    f.RedTime = (int)Math.Round(r.Where(w => f.InRange(w.Start)).Sum(s => s.Span.TotalSeconds), MidpointRounding.AwayFromZero);
                    f.YellowTime = (int)Math.Round(y.Where(w => f.InRange(w.Start)).Sum(s => s.Span.TotalSeconds), MidpointRounding.AwayFromZero);
                    f.GreenTime = (int)Math.Round(g.Where(w => f.InRange(w.Start)).Sum(s => s.Span.TotalSeconds), MidpointRounding.AwayFromZero);


                    _output.WriteLine($"cycle: {f}");
                }
            }






            //var sut = new AggregateDetectorEventsStep(tl);

            //var actual = await sut.ExecuteAsync(testData);

            //_output.WriteLine($"actual count: {actual.Count()}");

            //foreach (var a in actual)
            //{
            //    _output.WriteLine($"actual: {a}");
            //}

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
