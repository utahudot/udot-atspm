#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/GroupApproachesByLocationTests.cs
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class GroupApproachesByLocationTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public GroupApproachesByLocationTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        /// <summary>
        /// Tests that it's cancelling correctly, should return a <see cref="TaskCanceledException"/>
        /// </summary>
        [Fact]
        [Trait(nameof(GroupApproachesByLocation), "Cancellation")]
        public async void GroupApproachesByLocationTestCancellation()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            var testLogs = new List<IndianaEvent>
            {
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testLocation, testLogs);

            var sut = new GroupApproachesByLocation();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(testData, source.Token));
        }

        /// <summary>
        /// Tests the correct number of approaches are extracted from the phaseEvents Location
        /// </summary>
        [Fact]
        [Trait(nameof(GroupApproachesByLocation), "Approaches")]
        public async void GroupApproachesByLocationTestApproaches()
        {
            var testLogs = new List<IndianaEvent>
            {
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testLocation, testLogs);

            var sut = new GroupApproachesByLocation();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testLocation.Approaches.Select(s => Tuple.Create(s, 0, testLogs));

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests that the correct sort order of the events has been applied
        /// </summary>
        [Fact]
        [Trait(nameof(GroupApproachesByLocation), "Sort Order")]
        public async void GroupApproachesByLocationTestSortOrder()
        {
            var testLogs = new List<IndianaEvent>
            {
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testLocation, testLogs);

            var sut = new GroupApproachesByLocation();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testLocation.Approaches.Select(s => Tuple.Create(s, 0, testLogs.OrderBy(o => o.Timestamp).AsEnumerable()));

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests that only events with a LocationIdentifier matching the phaseEvents Location are forwarded
        /// </summary>
        [Fact]
        [Trait(nameof(GroupApproachesByLocation), "Location")]
        public async void GroupApproachesByLocationTestLocation()
        {
            var testLogs = new List<IndianaEvent>
            {
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testLocation, testLogs);

            var sut = new GroupApproachesByLocation();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testLocation.Approaches.Select(s => Tuple.Create(s, 0, testLogs.Where(w => w.LocationIdentifier == _testLocation.LocationIdentifier)));

            Assert.Equal(expected, actual);
        }






        [Fact]
        public async void Stuff()
        {
            var random = new Random();
            var eventCodes = Enum.GetValues(typeof(IndianaEnumerations))
                .Cast<short>()
                .ToList();

            var testLogs = Enumerable.Range(0, 100)
                .Select(s => new IndianaEvent()
                {
                    LocationIdentifier = _testLocation.LocationIdentifier,
                    Timestamp = DateTime.Now.AddSeconds(-random.Next(0, 100)),
                    EventCode = (short)eventCodes[random.Next(0, eventCodes.Count - 1)],
                    EventParam = (short)random.Next(0, 8)
                })
                .OrderBy(o => o.Timestamp)
                .ToList();

            //var filteredLogs = testLogs
            //    .Where(t =>
            //    !Enum.IsDefined(typeof(IndianaEnumerations), (ushort)t.EventCode) ||
            //    !(((IndianaEnumerations)t.EventCode)
            //    .GetAttributeOfType<IndianaEventLayerAttribute>()?.IndianaEventParamType == IndianaEventParamType.PhaseNumber))
            //    .ToList();


            //foreach (var t in testLogs)
            //{
            //    if (Enum.IsDefined(typeof(IndianaEnumerations), (ushort)t.EventCode))
            //    {
            //        var en = (IndianaEnumerations)t.EventCode;
            //        var att = en.GetAttributeOfType<IndianaEventLayerAttribute>();

            //        if (att != null && att.IndianaEventParamType == IndianaEventParamType.PhaseNumber)
            //            testLogs.Remove(t);
            //    }
            //}


            //if (att?.IndianaEventParamType == IndianaEventParamType.PhaseNumber &&



            var test = testLogs.GroupEventsByParamType();

            foreach (var k in test.Select(s => s.Key))
            {
                _output.WriteLine($"{k} - {test[k].Count()}");
            }

            //option 1
            //     return _testLocation.Approaches
            //.Select(a =>
            //    phaseEvents
            //        .Where(p => p.EventParam == a.ProtectedPhaseNumber)
            //        .Union(nonPhaseEvents)
            //        .ToList());

            //     //option 2
            //     var phaseLookup = phaseEvents.ToLookup(p => p.EventParam);
            //     return _testLocation.Approaches.Select(a =>
            //         phaseLookup[a.ProtectedPhaseNumber].Union(nonPhaseEvents).ToList()
            //     );




            //var nonPhaseEvents = testLogs
            //    .Where(t =>
            //    Enum.IsDefined(typeof(IndianaEnumerations), (ushort)t.EventCode) &&
            //    !(((IndianaEnumerations)t.EventCode)
            //    .GetAttributeOfType<IndianaEventLayerAttribute>()?.IndianaEventParamType == IndianaEventParamType.PhaseNumber))
            //    .ToList();

            //var phaseEvents = testLogs
            //    .Where(t =>
            //    Enum.IsDefined(typeof(IndianaEnumerations), (ushort)t.EventCode) &&
            //    (((IndianaEnumerations)t.EventCode)
            //    .GetAttributeOfType<IndianaEventLayerAttribute>()?.IndianaEventParamType == IndianaEventParamType.PhaseNumber))
            //    .ToList();

            //_output.WriteLine($"a: {nonPhaseEvents.Count} b: {phaseEvents.Count} --- {testLogs.Count}");

            //var joins = _testLocation.Approaches
            //    .GroupJoin(phaseEvents,
            //    o => o.ProtectedPhaseNumber,
            //    i => i.EventParam,
            //    (o, i) => i)
            //    .ToList();


            //foreach (var j in joins)
            //{
            //    _output.WriteLine($"before: {j.Count()}");

            //    var yoda = j.Union(nonPhaseEvents);

            //    _output.WriteLine($"after: {yoda.Count()}");
            //}







            //foreach (var n in nonPhaseEvents)
            //{
            //    if (Enum.IsDefined(typeof(IndianaEnumerations), (ushort)n.EventCode))
            //    {
            //        var en = (IndianaEnumerations)n.EventCode;
            //        var att = en.GetAttributeOfType<IndianaEventLayerAttribute>();

            //        _output.WriteLine($"nonPhaseEvents: {n} --- {att?.IndianaEventParamType}");
            //    }
            //}

            //foreach (var n in phaseEvents)
            //{
            //    if (Enum.IsDefined(typeof(IndianaEnumerations), (ushort)n.EventCode))
            //    {
            //        var en = (IndianaEnumerations)n.EventCode;
            //        var att = en.GetAttributeOfType<IndianaEventLayerAttribute>();

            //        _output.WriteLine($"nonPhaseEvents: {n} --- {att?.IndianaEventParamType}");
            //    }
            //}








            //foreach (var t in phaseEvents)
            //{
            //    _output.WriteLine($"group");

            //    foreach (var a in t)
            //    {
            //        _output.WriteLine($"{a}");
            //    }
            //}



            //var testData = Tuple.Create(_testLocation, testLogs.AsEnumerable());

            //var sut = new GroupApproachesByLocation();

            //var actual = await sut.ExecuteAsync(testData);

            //var expected = _testLocation.Approaches.Select(s => Tuple.Create(s, s.ProtectedPhaseNumber, testLogs.Where(w => w.LocationIdentifier == _testLocation.LocationIdentifier)));

            //Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
        }
    }

    public static class EnumExtensions
    {
        /// <summary>
        /// Gets an attribute on an enum field value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to retrieve.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>
        /// The attribute of the specified type or null.
        /// </returns>
        public static T GetAttributeOfType<T>(this Enum enumValue) where T : Attribute
        {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString()).First();
            var attributes = memInfo.GetCustomAttributes<T>(false);
            return attributes.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all custom attributes of a specified type applied to an enum field.
        /// </summary>
        /// <typeparam name="T">The type of attribute to retrieve.</typeparam>
        /// <param name="enumValue">The enum value whose attributes are to be retrieved.</param>
        /// <returns>
        /// A read-only list containing all attributes of the specified type applied to the enum field.
        /// Returns an empty list if no such attributes are found.
        /// </returns>
        public static IReadOnlyList<T> GetAttributesOfType<T>(this Enum enumValue) where T : Attribute
        {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString()).First();
            var attributes = memInfo.GetCustomAttributes<T>(false);
            return attributes.ToList();
        }
    }
}
