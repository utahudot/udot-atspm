#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/CalculateTotalVolumesTests.cs
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

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
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

        [Fact()]
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

        private IReadOnlyList<IndianaEvent> KeepFirstSequentialEvent(IEnumerable<IndianaEvent> events, IndianaEnumerations eventCode)
        {
            var sort = events.OrderBy(o => o.Timestamp).ToList();
            
            return sort.Where((w, i) => i == 0 || w.EventCode != (int)eventCode || (w.EventCode == (int)eventCode && w.EventCode != sort[i - 1].EventCode)).ToList();
        }

        [Theory]
        [AnalysisTestData<IdentifyPedCyclesTestData>]
        [Trait(nameof(IdentifyPedCycles), "From File")]
        public void CalculateTotalVolumesIdentifyPedCyclesFromFileTest(Location config, List<IndianaEvent> input, object output)
        {
            _output.WriteLine($"{config} - {input.Count(c => c.EventCode == 21)}");

            var filter = input
                .Where(w => w.EventCode == 21 || w.EventCode == 22 || w.EventCode == 90)
                .OrderBy(o => o.Timestamp)
                .ToList();


            var test = KeepFirstSequentialEvent(filter, IndianaEnumerations.PedDetectorOn).ToList();

            var results = test
                .Select((t, index) => new { Item = t, Index = index })
                .Where(x => x.Item.EventCode == 90 && x.Index > 0 && x.Index < test.Count - 1)
                .Select(x =>
                {
                    var prev = test[x.Index - 1];
                    var next = test[x.Index + 1];

                    var pedCycle = new PedCycle()
                    {
                        PedDetected = x.Item.Timestamp
                    };

                    double delay = 0;

                    if (prev.EventCode == 21 && next.EventCode == 22)
                        pedCycle.BeginWalk = prev.Timestamp;
                    else if (prev.EventCode == 21 && next.EventCode == 21)
                        pedCycle.BeginWalk = next.Timestamp;
                    else if (prev.EventCode == 22 && next.EventCode == 21)
                        pedCycle.BeginWalk = next.Timestamp;
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
                //f.LocationIdentifier = config.LocationIdentifier;
                //f.PhaseNumber = 2; // Assuming phase number 2 for pedestrian phases
                //f.ApproachId = config.Approaches.FirstOrDefault()?.ApproachId ?? 0; // Assuming first approach
                f.PedCycles = results.Count(c => f.InRange(c.BeginWalk));
                f.PedDelay = results.Where(c => f.InRange(c.BeginWalk)).Sum(s => s.PedDelay);

                //these aren't working for all of them
                f.MinPedDelay = results.Where(c => f.InRange(c.BeginWalk)).Select(s => s.PedDelay).DefaultIfEmpty(0).Min();
                f.MaxPedDelay = results.Where(c => f.InRange(c.BeginWalk)).Select(s => s.PedDelay).DefaultIfEmpty(0).Max();

                f.PedRequests = input.Count(w => w.EventCode == 90 && f.InRange(w.Timestamp));
            });

            _output.WriteLine($"{tl.Segments.ToList().Count}");
            foreach (var r in tl.Segments.ToList())
            {
                if (r.PedCycles > 0 || r.PedRequests > 0)
                    _output.WriteLine($"{r}");
            }

            //foreach (var t in test)
            //{
            //    if (t.EventCode == 90)
            //    {
            //        var i = test.IndexOf(t);

            //        if (i > 0 && i <= test.Count)
            //        {
            //            double delay = 0;

            //            if (test[i - 1].EventCode == 21 && test[i + 1].EventCode == 22)
            //                delay = 0;

            //            if (test[i - 1].EventCode == 21 && test[i + 1].EventCode == 21)
            //                delay = Math.Abs((test[i + 1].Timestamp - t.Timestamp).TotalSeconds);

            //            if (test[i - 1].EventCode == 22 && test[i + 1].EventCode == 21)
            //                delay = Math.Abs((test[i + 1].Timestamp - t.Timestamp).TotalSeconds);

            //            _output.WriteLine($"{test[i - 1]} - {test[i]} - {test[i + 1]} : {delay}");
            //        }
            //    }
            //}


            //var preFilter = KeepFirstSequentialEvent(filter, IndianaEnumerations.PedestrianBeginChangeInterval);

            //_output.WriteLine($"counts: {filter.Count} - {preFilter.Count} - {DateTime.MinValue}");

            //preFilter = preFilter.Where(w => w.EventCode == 22).ToList();

            //var test = preFilter
            //    .Where((w, i) => i < preFilter.Count - 1 && w.EventCode == 22)
            //    .Select((s, i) => new PedCycle()
            //    {
            //        Cycle = i + 1,
            //        Start = preFilter[i].Timestamp,
            //        BeginChangeInterval = preFilter[i + 1].Timestamp,
            //        End = preFilter[i + 1].Timestamp
            //    }).ToList();

            //test.ForEach(f =>
            //{
            //    f.BeginWalk = filter.FirstOrDefault(w => w.EventCode == 21 && f.InRange(w.Timestamp)).Timestamp;

            //    var peds = filter.Where(w => w.EventCode == 90 && f.InRange(w.Timestamp));

            //    if (peds.Any())
            //        f.PedDetected = peds.FirstOrDefault().Timestamp;
            //});



            //var test = filter
            //    .Select((s, i) =>
            //{
            //    if (s.EventCode == 21)
            //    {
            //        cycle++;

            //        var test = new PedCycle()
            //        {
            //            Cycle = cycle,
            //            BeginWalk = s.Timestamp,
            //        };

            //        if (i < filter.Count - 1 && filter[i + 1].EventCode == 22)
            //        {

            //            test.BeginChangeInterval = filter[i + 1].Timestamp;
            //        }

            //        if (i < filter.Count - 2 && filter[i + 2].EventCode == 22)
            //        {
            //            test.BeginChangeInterval = filter[i + 2].Timestamp;
            //        }

            //        return test;
            //    }
            //    else
            //    {
            //        return null;
            //    }

            //})
            //    .Where(w => w != null)
            //    .ToList();

            //_output.WriteLine($"{test.Min(m => m.Cycle)} - {test.Max(m => m.Cycle)} --- {test.Count()}");

            //foreach (var t in test.Where(w => w.Detected))
            //{
            //    _output.WriteLine($"{t}");
            //}



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
