#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis.WorkflowFilterTests/WorkflowFilterTestsBase.cs
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
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public abstract class WorkflowFilterTestsBase : IDisposable
    {
        protected readonly ITestOutputHelper _output;

        protected List<int> filteredList = new();
        protected FilterEventCodeLocationBase sut;

        protected Tuple<Location, IEnumerable<ControllerEventLog>> testData;

        public WorkflowFilterTestsBase(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CheckFilterPass()
        {
            var testLocation = new Location() { LocationIdentifier = "1001" };
            var testLogs = Enumerable.Range(0, 1000).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = testLocation.LocationIdentifier,
                Timestamp = DateTime.Now.AddSeconds(s),
                EventCode = s,
                EventParam = 1
            }).ToList();

            testData = Tuple.Create(testLocation, testLogs.AsEnumerable());

            sut.Post(testData);
            sut.Complete();

            var actual = sut.Receive();
            var expected = Tuple.Create(testLocation, testLogs.Where(w => filteredList.Contains(w.EventCode)));

            //foreach (var a in actual.Item2)
            //    _output.WriteLine($"actual: {a}");

            //foreach (var e in expected.Item2)
            //    _output.WriteLine($"expected: {e}");

            Assert.Equal(expected, actual);
        }

        public virtual void Dispose()
        {
            //testData = null;
        }
    }
}
