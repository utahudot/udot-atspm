#region license
// Copyright 2026 Utah Departement of Transportation
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

using System;
using System.Collections.Generic;
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
    public class AggregateDetectorEventsTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public AggregateDetectorEventsTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
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
