#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowFilterTests/WorkflowFilterTestsBase.cs
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
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowFilterTests
{
    public abstract class WorkflowFilterTestsBase : IDisposable
    {
        protected readonly ITestOutputHelper _output;

        protected HashSet<short> filteredCodes;
        protected FilterIndianaEventsByCodeAndLocationBase sut;

        private readonly short _maxEventCode;

        public WorkflowFilterTestsBase(ITestOutputHelper output)
        {
            _output = output;
            _maxEventCode = Enum.GetValues(typeof(IndianaEnumerations)).Cast<short>().Max();
        }

        [Fact]
        [Trait(nameof(FilterIndianaEventsByCodeAndLocationBase), "FilteredCodes")]
        public void WorkflowFilterTestsFilteredCodes()
        {
            var testLocation = new Location() { LocationIdentifier = "1001" };

            var testLogs = Enumerable.Range(0, _maxEventCode).Select(s => new IndianaEvent()
            {
                LocationIdentifier = testLocation.LocationIdentifier,
                Timestamp = DateTime.Now.AddSeconds(s),
                EventCode = Convert.ToInt16(s),
                EventParam = 1
            }).ToList();

            var testData = Tuple.Create(testLocation, testLogs.AsEnumerable());

            sut.Post(testData);
            sut.Complete();

            var actual = sut.Receive();
            var expected = Tuple.Create(testLocation, testLogs.Where(w => filteredCodes.Contains(w.EventCode)).OrderBy(o => o.Timestamp).AsEnumerable());

            //foreach (var a in actual.Item2)
            //    _output.WriteLine($"actual: {a}");

            //foreach (var e in expected.Item2)
            //    _output.WriteLine($"expected: {e}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(FilterIndianaEventsByCodeAndLocationBase), "LocationIdentifier(")]
        public void WorkflowFilterTestsLocationIdentifier()
        {
            var testLocation1 = new Location() { LocationIdentifier = "1001" };
            var testLocation2 = new Location() { LocationIdentifier = "1002" };

            var coorectLogs = filteredCodes.Select(s => new IndianaEvent()
            {
                LocationIdentifier = testLocation1.LocationIdentifier,
                Timestamp = DateTime.Now.AddSeconds(s),
                EventCode = s,
                EventParam = 1
            }).ToList();

            var inCorrectLogs = filteredCodes.Select(s => new IndianaEvent()
            {
                LocationIdentifier = testLocation2.LocationIdentifier,
                Timestamp = DateTime.Now.AddSeconds(s),
                EventCode = s,
                EventParam = 1
            }).ToList();

            var testData = Tuple.Create(testLocation1, coorectLogs.Union(inCorrectLogs));

            sut.Post(testData);
            sut.Complete();

            var actual = sut.Receive();
            var expected = Tuple.Create(testLocation1, coorectLogs.OrderBy(o => o.Timestamp).AsEnumerable());

            //foreach (var a in actual.Item2)
            //    _output.WriteLine($"actual: {a}");

            //foreach (var e in expected.Item2)
            //    _output.WriteLine($"expected: {e}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(FilterIndianaEventsByCodeAndLocationBase), "TimestampOrder(")]
        public void WorkflowFilterTestsTimestampOrder()
        {
            var testLocation = new Location() { LocationIdentifier = "1001" };

            var testLogs = Enumerable.Range(0, _maxEventCode).Select(s => new IndianaEvent()
            {
                LocationIdentifier = testLocation.LocationIdentifier,
                Timestamp = DateTime.Now.AddSeconds(Random.Shared.Next(500)),
                EventCode = Convert.ToInt16(s),
                EventParam = 1
            }).ToList();

            var testData = Tuple.Create(testLocation, testLogs.AsEnumerable());

            sut.Post(testData);
            sut.Complete();

            var actual = sut.Receive();
            var expected = Tuple.Create(testLocation, actual.Item2.OrderBy(o => o.Timestamp).AsEnumerable());

            //foreach (var a in actual.Item2)
            //    _output.WriteLine($"actual: {a}");

            //foreach (var e in expected.Item2)
            //    _output.WriteLine($"expected: {e}");

            Assert.Equal(expected, actual);
        }

        public virtual void Dispose()
        {
        }
    }
}
