#region license
// Copyright 2026 Utah Departement of Transportation
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
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class AggregatePriorityStepTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public AggregatePriorityStepTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }


        //[Fact(Skip = "Used to create test data")]
        [Fact]
        public async Task Stuff()
        {
            {
                //var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\Location7115TestData.json").FullName);
                //var Location = JsonConvert.DeserializeObject<Location>(json);

                var file1 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\7707-priortiy-raw.csv");

                var logs = File.ReadAllLines(file1.FullName)
                       .Skip(1)
                       .Select(x => x.Split(','))
                       .Select(x => new IndianaEvent
                       {
                           //LocationIdentifier = x[0],
                           LocationIdentifier = "7115",
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

                var file2 = new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\priorityaggresult.csv");

                var output = File.ReadAllLines(file2.FullName)
                       .Skip(1)
                       .Select(x => x.Split(','))
                       .Select(x => new PriorityAggregation
                       {
                           Start = DateTime.Parse(x[0]),
                           End = DateTime.Parse(x[1]).AddMinutes(15),
                           LocationIdentifier = "7115",
                           PriorityNumber = int.Parse(x[3]),
                           PriorityRequests = int.Parse(x[4]),
                           PriorityServiceEarlyGreen = int.Parse(x[5]),
                           PriorityServiceExtendedGreen = int.Parse(x[6]),

                       }).ToList();

                _output.WriteLine($"{output.Count}");

                foreach (var o in output)
                {
                    _output.WriteLine($"{o}");
                }

                var result = new AggregatePriorityTestData()
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
                File.WriteAllText(@"C:\Users\christianbaker\source\repos\udot-atspm\Atspm\ApplicationTests\Analysis\TestData\AggregatePriorityTestData1.json", test);
            }
        }








        [Theory]
        [AnalysisTestData<AggregatePriorityTestData>]
        [Trait(nameof(AggregatePriorityStep), "From File")]
        public async Task AggregatePedestrianPhasesFromFileTest(Location config, IEnumerable<IndianaEvent> input, IEnumerable<PriorityAggregation> output)
        {
            var testData = Tuple.Create(config, input);

            var aggDate = input
                .GroupBy(dt => dt.Timestamp)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault().Key;

            var tl = aggDate.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));

            var sut = new AggregatePriorityStep(tl);

            var temp = await sut.ExecuteAsync(testData);
            var actual = temp.ToList();

            _output.WriteLine($"actual: {actual.Count()}");

            var expected = output.ToList();

            _output.WriteLine($"expected: {expected.Count()}");

            //Assert.Equivalent(actual, expected);

            int maxCount = Math.Max(expected.Count, actual.Count);

            Assert.Multiple(() =>
            {
                Assert.Equal(expected.Count, actual.Count);

                int maxCount = Math.Max(expected.Count, actual.Count);

                for (int i = 0; i < maxCount; i++)
                {
                    var exp = i < expected.Count ? expected[i] : null;
                    var act = i < actual.Count ? actual[i] : null;

                    // Check if objects match by comparing serialized JSON or properties
                    bool isMatch = System.Text.Json.JsonSerializer.Serialize(exp) ==
                                   System.Text.Json.JsonSerializer.Serialize(act);

                    Assert.True(
                        isMatch,
                        $"[INDEX {i} MISMATCH]\n  Expected: {System.Text.Json.JsonSerializer.Serialize(exp)}\n  Actual:   {System.Text.Json.JsonSerializer.Serialize(act)}"
                    );
                }
            });
        }

        public void Dispose()
        {
        }
    }
}
