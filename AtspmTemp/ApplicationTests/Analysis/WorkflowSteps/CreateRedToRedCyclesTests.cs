#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis.WorkflowSteps/CreateRedToRedCyclesTests.cs
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
using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class CreateRedToRedCyclesTests : IClassFixture<TestApproachFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Approach _testApproach;

        public CreateRedToRedCyclesTests(ITestOutputHelper output, TestApproachFixture testApproach)
        {
            _output = output;
            _testApproach = testApproach.TestApproach;
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Location Filter")]
        public async void CreateRedToRedCyclesLocationFilterTest()
        {
            var correct = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            };

            var incorrect = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            };

            var testLogs = correct.Union(incorrect);

            foreach (var l in testLogs)
            {
                _output.WriteLine($"logs: {l}");
            }

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var expected = correct.Select(s => s.LocationIdentifier).Distinct().OrderBy(o => o);
            var actual = result.Item2.Select(s => s.LocationIdentifier).Distinct().OrderBy(o => o);

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Approach Filter")]
        public async void CreateRedToRedCyclesApproachFilterTest()
        {
            var correct = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            };

            var incorrect = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 5 },
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 5 },
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 5 },
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 5 },
            };

            var testLogs = correct.Union(incorrect);

            foreach (var l in testLogs)
            {
                _output.WriteLine($"logs: {l}");
            }

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var expected = correct.Select(s => s.EventParam).Distinct().OrderBy(o => o);
            var actual = result.Item2.Select(s => Convert.ToInt16(s.PhaseNumber)).Distinct().OrderBy(o => o);

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Code Filter")]
        public async void CreateRedToRedCyclesCodeFilterTest()
        {
            var correct = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            };

            var incorrect = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 101, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 102, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 103, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 104, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            };

            var testLogs = correct.Union(incorrect);

            foreach (var l in testLogs)
            {
                _output.WriteLine($"logs: {l}");
            }

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var expected = new RedToRedCycle()
            {
                Start = correct[0].Timestamp,
                GreenEvent = correct[1].Timestamp,
                YellowEvent = correct[2].Timestamp,
                End = correct[3].Timestamp
            };

            var actual = result.Item2.First();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.GreenEvent, actual.GreenEvent);
            Assert.Equal(expected.YellowEvent, actual.YellowEvent);
            Assert.Equal(expected.End, actual.End);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Data Check")]
        public async void CreateRedToRedCyclesDataCheckTest()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var actual = result.Item2.First();
            var expected = new RedToRedCycle()
            {
                LocationIdentifier = _testApproach.Location.LocationIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Start = DateTime.Parse("4/17/2023 8:01:48.8"),
                GreenEvent = DateTime.Parse("4/17/2023 8:03:11.7"),
                YellowEvent = DateTime.Parse("4/17/2023 8:04:13.7"),
                End = DateTime.Parse("4/17/2023 8:04:18.8")
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Timestamp Order")]
        public async void CreateRedToRedCyclesOrderTest()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var actual = result.Item2.First();
            var expected = new RedToRedCycle()
            {
                Start = DateTime.Parse("4/17/2023 8:01:48.8"),
                GreenEvent = DateTime.Parse("4/17/2023 8:03:11.7"),
                YellowEvent = DateTime.Parse("4/17/2023 8:04:13.7"),
                End = DateTime.Parse("4/17/2023 8:04:18.8")
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.GreenEvent, actual.GreenEvent);
            Assert.Equal(expected.YellowEvent, actual.YellowEvent);
            Assert.Equal(expected.End, actual.End);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No Start Event")]
        public async void CreateRedToRedCyclesNoStartTest()
        {
            var testLogs = new List<IndianaEvent>
            {
                //new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var actual = result.Item2.Count();
            var expected = 0;

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No End Event")]
        public async void CreateRedToRedCyclesNoEndTest()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                //new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var actual = result.Item2.Count();
            var expected = 0;

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No Green Event")]
        public async void CreateRedToRedCyclesNoGreenTest()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                //new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var actual = result.Item2.Count();
            var expected = 0;

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No Yellow Event")]
        public async void CreateRedToRedCyclesNoYellowTest()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                //new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var actual = result.Item2.Count();
            var expected = 0;

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Event Order")]
        public async void CreateRedToRedCyclesEventOrderTest()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
                new IndianaEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = Convert.ToInt16(_testApproach.ProtectedPhaseNumber)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            var cycle = result.Item2.First();

            var condition = cycle.Start < cycle.GreenEvent && cycle.GreenEvent < cycle.YellowEvent && cycle.YellowEvent < cycle.End;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Null Input")]
        public async void CreateRedToRedCyclesNullInputTest()
        {
            var testData = Tuple.Create<Approach, IEnumerable<IndianaEvent>>(null, null);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            var condition = result != null && result.Item1 == null && result.Item2 == null;

            _output.WriteLine($"condition: {condition}");

            Assert.True(condition);

            Assert.True(result != null);
            Assert.True(result.Item1 == null);
            Assert.True(result.Item2 == null);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No Data")]
        public async void CreateRedToRedCyclesNoDataTest()
        {
            var testLogs = Enumerable.Range(1, 5).Select(s => new IndianaEvent()
            {
                LocationIdentifier = "1001",
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = Convert.ToInt16(Random.Shared.Next(1, 50)),
                EventParam = 5
            });

            foreach (var l in testLogs)
            {
                _output.WriteLine($"logs: {l}");
            }

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"cycle: {l}");
            }

            Assert.True(result != null);
            Assert.True(result.Item1 == _testApproach);
            Assert.True(result.Item2?.Count() == 0);
        }

        [Theory]
        [InlineData(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CreateRedToRedCyclesTestData1.json")]
        [Trait(nameof(CreateRedToRedCycles), "From File")]
        public async void CreateRedToRedCyclesFromFileTest(string file)
        {
            var json = File.ReadAllText(new FileInfo(file).FullName);
            var testFile = JsonConvert.DeserializeObject<RedToRedCyclesTestData>(json);

            _output.WriteLine($"Configuration: {testFile.Configuration}");
            _output.WriteLine($"Input: {testFile.Input.Count}");
            _output.WriteLine($"Output: {testFile.Output.Count}");

            var testData = Tuple.Create(testFile.Configuration, testFile.Input.AsEnumerable());

            var sut = new CreateRedToRedCycles();

            var result = await sut.ExecuteAsync(testData);

            var expected = testFile.Output;
            var actual = result.Item2.ToList();

            _output.WriteLine($"expected: {expected.Count}");
            _output.WriteLine($"actual: {actual.Count}");

            Assert.Equivalent(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
