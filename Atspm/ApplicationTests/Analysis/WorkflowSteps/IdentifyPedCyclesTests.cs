#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/IdentifyPedCyclesTests.cs
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
using System.Threading.Tasks;
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
    public class IdentifyPedCyclesTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public IdentifyPedCyclesTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        [Fact(Skip = "use to generate test files")]
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



                //var c = new CalculateDwellTime();

                //var r = await c.ExecuteAsync(Tuple.Create(Location, logs.AsEnumerable(), 1));

                var result = new IdentifyPedCyclesTestData()
                {
                    Configuration = _testLocation,
                    Input = logs,
                    Output = null
                };


                var test = JsonConvert.SerializeObject(result, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                });
                File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\IdentifyPedCyclesTestData1.json", test);
            }
        }



        [Theory]
        [AnalysisTestData<IdentifyPedCyclesTestData>]
        [Trait(nameof(IdentifyPedCycles), "From File")]
        public void CalculateTotalVolumesIdentifyPedCyclesFromFileTest(Location config, List<IndianaEvent> input, object output)
        {
            //90, 67, 68 params aren't for phase? btw, there are no 67's in test data
            //how to get phase number?
            //aggregation doesn't have approachid only phase number
            //aggregation is int for peddelay and min max?
            //aggregation all 15 min into bins or just the ones with data?
            //if we are going to start on 22, then excel should be updated?
            //if i use an attribute to organize events into phases, would that affect anything?


            //var approaches = config.Approaches.ToLookup(l => l.ProtectedPhaseNumber);


            _output.WriteLine($"{config} - {input.Count(c => c.EventCode == 21)}");

            var filter = input
                .Where(w => w.EventCode == 21 || w.EventCode == 22 || w.EventCode == 90)
                .OrderBy(o => o.Timestamp)
                .ToList();


            var test = filter.KeepFirstSequentialEvent(IndianaEnumerations.PedDetectorOn).ToList();

            var results = test
                .Select((t, index) => new { Item = t, Index = index })
                .Where(x => x.Item.EventCode == 90 && x.Index > 0 && x.Index < test.Count - 1)
                .Select(x =>
                {
                    var prev = test[x.Index - 1];
                    var next = test[x.Index + 1];

                    var pedCycle = new PedCycle()
                    {
                        PedDetectorOn = x.Item.Timestamp
                    };

                    if (prev.EventCode == 21 && next.EventCode == 22)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = prev.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    else if (prev.EventCode == 21 && next.EventCode == 21)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = next.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    else if (prev.EventCode == 22 && next.EventCode == 21)
                    {
                        pedCycle.Start = prev.Timestamp;
                        pedCycle.BeginWalk = next.Timestamp;
                        pedCycle.End = next.Timestamp;
                    }

                    return pedCycle;
                })
                .Where(w => w.BeginWalk > DateTime.MinValue)
                .ToList();


            _output.WriteLine($"{results.Min(m => m.BeginWalk)} - {results.Max(m => m.BeginWalk)}");

            //foreach (var r in results)
            //{
            //    _output.WriteLine($"{r}");
            //}

            var span = TimeSpan.FromMinutes(15);

            //HACK: updated timeline object in toolkit so the rounding can be removed
            var tl = new Timeline<PhasePedAggregation>(results.Min(m => m.BeginWalk).RoundDown(span), results.Max(m => m.BeginWalk).RoundUp(span), span);

            _output.WriteLine($"{tl.Start} - {tl.End}");

            tl.Segments.ToList().ForEach(f =>
            {
                f.LocationIdentifier = config.LocationIdentifier;
                f.PhaseNumber = 2;
                f.PedCycles = results.Count(c => f.InRange(c.BeginWalk));
                f.PedDelay = results.Where(c => f.InRange(c.BeginWalk)).Sum(s => s.PedDelay);

                f.MinPedDelay = results.Where(c => f.InRange(c.BeginWalk)).Select(s => s.PedDelay).Where(w => w > 0).DefaultIfEmpty(0).Min();
                f.MaxPedDelay = results.Where(c => f.InRange(c.BeginWalk)).Select(s => s.PedDelay).DefaultIfEmpty(0).Max();


                f.PedRequests = input.Count(w => w.EventCode == 90 && f.InRange(w.Timestamp));

                var imputedCalls = input.Where(w => w.EventCode == 90 || w.EventCode == 21 || w.EventCode == 67 || w.EventCode == 0).OrderBy(o => o.Timestamp).ToList();
                f.ImputedPedCallsRegistered = imputedCalls.Where((w, i) => i > 0 && f.InRange(w.Timestamp) && w.EventCode == 90 && (imputedCalls[i - 1].EventCode == 21 || imputedCalls[i - 1].EventCode == 67 || imputedCalls[i - 1].EventCode == 0)).Count();



                var uniquePedDetections = input
                .Where(w => w.EventCode == 90)
                .OrderBy(o => o.Timestamp)
                .ToList();

                f.UniquePedDetections = uniquePedDetections.SlidingWindow(2)
                .Where(x => x[1].Timestamp - x[0].Timestamp > TimeSpan.FromSeconds(15))
                .Count(w => f.InRange(w[1].Timestamp));



                f.PedBeginWalkCount = input.Count(w => w.EventCode == 21 && f.InRange(w.Timestamp));


                f.PedCallsRegisteredCount = input.Count(w => w.EventCode == 45 && f.InRange(w.Timestamp));
            });



            _output.WriteLine($"{tl.Segments.ToList().Count}");

            foreach (var r in tl.Segments.ToList())
            {
                if (r.PedCycles > 0 || r.PedRequests > 0)
                    _output.WriteLine($"{r}");
            }





            //var json = File.ReadAllText(new FileInfo(file).FullName);
            //var testFile = JsonConvert.DeserializeObject<IdentifyPedCyclesTestData>(json);

            //_output.WriteLine($"Configuration: {testFile.Configuration}");
            //_output.WriteLine($"Input: {testFile.Input.Count}");
            //_output.WriteLine($"Output: {testFile.Output.Segments.Count}");

            //var t1 = Tuple.Create(testFile.Configuration[0], testFile.Input[0]);
            //var t2 = Tuple.Create(testFile.Configuration[1], testFile.Input[1]);

            //var testData = Tuple.Create(t1, t2);

            //var sut = new CalculateTotalVolumes();

            //var result = await sut.ExecuteAsync(testData);

            //var expected = testFile.Output;
            //var actual = result.Item2;

            //_output.WriteLine($"expected: {expected.Count}");
            //_output.WriteLine($"actual: {actual.Count}");

            //Assert.Equivalent(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
