#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/AggregatePhaseCyclesTests.cs
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

using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Business.Bins;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    //public class SplitMonitorData : StartEndRange
    //{
    //    public short PhaseNumber { get; set; }
    //}

    public class AggregateSplitMonitorTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public AggregateSplitMonitorTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        [Fact(Skip = "skip")]
        public async Task Stuff()
        {
            {
                var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\TempSplitMonitorTestData.csv");

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

                var result = new AggregatePhaseSplitMonitorData()
                {
                    Configuration = _testLocation,
                    Input = logs,
                    Output = new List<PhaseSplitMonitorAggregation>()
                };

                var test = JsonConvert.SerializeObject(result, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                });
                File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\AggregatePhaseSplitMonitorData1.json", test);
            }
        }

        [Fact]
        public async Task AggregateSplitMonitorTest()
        {
            var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\TempSplitMonitorTestData.csv");

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

            var cycleStarts = logs
                .Where(w => w.EventCode == 1 || w.EventCode == 11)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.EventParam)
                .SelectMany(g =>
                g.SlidingWindow(2)
                .Where(window => window.Select(e => e.EventCode).SequenceEqual(new List<short>() { 1, 11}))
                .Select(w => new 
                {
                    PhaseNumber = g.Key,
                    Start = w[0].Timestamp,
                    End = w[1].Timestamp
                }))
                .GroupBy(x => x.PhaseNumber)
                .ToList();

            var tl = DateTime.Parse("2/12/2025").CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));

            var bin = cycleStarts.SelectMany(phaseGroup =>
                tl.Segments.Select(s =>
                {
                    var durations = phaseGroup
                        .Where(w => s.InRange(w.Start))
                        .Select(x => (x.End - x.Start).TotalSeconds)
                        .ToList();

                    var count = durations.Count;

                    var agg = new PhaseSplitMonitorAggregation
                    {
                        LocationIdentifier = "7115",
                        PhaseNumber = phaseGroup.Key,
                        Start = s.Start,
                        End = s.End,
                        EightyFifthPercentileSplit = count == 0 ? -1 : Math.Round(AtspmMath.Percentile(durations, 85), 1, MidpointRounding.AwayFromZero)
                    };

                    return (agg, count);
                })
            ).ToList();

            var maxCounts = bin.GroupBy(g => g.agg.Start).ToDictionary(d => d.Key, d => d.Max(x => x.count));

            foreach (var (agg, count) in bin)
            {
                agg.SkippedCount = maxCounts[agg.Start] - count;
            }

            var result = bin
                .Select(s => s.agg)
                .OrderBy(o => o.PhaseNumber)
                .ThenBy(t => t.Start)
                .ToList();

            foreach (var r in result)
            {
                _output.WriteLine($"result: {r.Start} - {r.LocationIdentifier} - {r.PhaseNumber} - {r.EightyFifthPercentileSplit} - {r.SkippedCount}");
            }
        }

        [Theory]
        [AnalysisTestData<AggregatePhaseSplitMonitorData>]
        [Trait(nameof(AggregatePhaseSplitMonitorData), "From File")]
        public async Task AggregatePhaseCyclesFromFileTest(Location config, IEnumerable<IndianaEvent> input, IEnumerable<PhaseSplitMonitorAggregation> output)
        {
            var testData = Tuple.Create(config, input);

            var aggDate = input
                .GroupBy(dt => dt.Timestamp)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault().Key;

            var tl = aggDate.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));

            var sut = new AggregatePhaseSplitMonitorStep(tl);

            var actual = await sut.ExecuteAsync(testData);












            _output.WriteLine($"actual: {actual.Count()}");

            foreach (var a in actual.Where(w => w.PhaseNumber == 2))
            {
                _output.WriteLine($"{a}");
            }

            var expected = output;

            _output.WriteLine($"expected: {expected.Count()}");

            Assert.Equivalent(actual, expected);
        }

        public void Dispose()
        {
        }
    }
}
