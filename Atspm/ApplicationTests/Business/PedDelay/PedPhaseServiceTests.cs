#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Business.PedDelay/PedPhaseServiceTests.cs
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Utah.Udot.Atspm.Business.PedDelay;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Business.PedDelay
{
    public class PedPhaseServiceTests
    {
        private const string LocationIdentifier = "6648";
        private static readonly DateTime Start = new(2026, 4, 6);

        [Theory]
        [InlineData(false, 21, 90, 22, 0)]
        [InlineData(false, 22, 90, 21, 8)]
        [InlineData(false, 21, 90, 21, 8)]
        [InlineData(true, 67, 90, 68, 0)]
        [InlineData(true, 68, 90, 67, 8)]
        [InlineData(true, 67, 90, 67, 8)]
        public void GetPedPhaseData_RecognizesDocumentedRequestSequences(
            bool isPedestrianPhaseOverlap,
            int firstCode,
            int secondCode,
            int thirdCode,
            double expectedDelay)
        {
            var events = CreateEvents(
                (10, firstCode),
                (12, secondCode),
                (20, thirdCode),
                (30, isPedestrianPhaseOverlap ? 68 : 22),
                (40, 90));

            var result = GetPedPhaseData(events, isPedestrianPhaseOverlap);

            var cycle = Assert.Single(result.Cycles);
            Assert.Equal(expectedDelay, cycle.Delay);
        }

        [Theory]
        [InlineData(false, 22, 21)]
        [InlineData(true, 68, 67)]
        public void GetPedPhaseData_CollapsesRepeatedRequestsAfterRemovingInterleavedEvents(
            bool isPedestrianPhaseOverlap,
            int beginClearanceCode,
            int beginWalkCode)
        {
            var walkTime = Start.AddSeconds(20);
            var events = CreateEvents(
                (10, beginClearanceCode),
                (12, 90),
                (12, 45),
                (12.5, 89),
                (13, 90),
                (13.5, 89),
                (20, beginWalkCode),
                (30, beginClearanceCode),
                (40, beginClearanceCode));

            var options = CreateOptions();
            var phaseData = GetPedPhaseData(events, isPedestrianPhaseOverlap, options);

            var cycle = Assert.Single(phaseData.Cycles);
            Assert.Equal(Start.AddSeconds(12), cycle.CallRegistered);
            Assert.Equal(walkTime, cycle.BeginWalk);
            Assert.Equal(8, cycle.Delay);
            Assert.Equal(2, phaseData.PedPresses);
            Assert.Equal(1, phaseData.PedRequests);
            Assert.Equal(1, phaseData.PedCallsRegisteredCount);
            Assert.Equal(1, phaseData.UniquePedDetections);
            Assert.DoesNotContain(phaseData.PedBeginWalkEvents, e => e.Timestamp == walkTime);

            options.ShowPedBeginWalk = true;
            var chart = new PedDelayService().GetChartData(options, phaseData, []);
            var startOfWalk = Assert.Single(chart.StartOfWalk);
            Assert.Equal(walkTime, startOfWalk.Timestamp);
            Assert.Equal(8, startOfWalk.Value);
        }

        [Theory]
        [InlineData(false, 21, 22)]
        [InlineData(true, 67, 68)]
        public void GetPedPhaseData_PreservesLeadingRequestBoundaryCase(
            bool isPedestrianPhaseOverlap,
            int beginWalkCode,
            int beginClearanceCode)
        {
            var events = CreateEvents(
                (10, 90),
                (20, beginWalkCode),
                (30, beginClearanceCode),
                (40, 90),
                (50, beginClearanceCode));

            var result = GetPedPhaseData(events, isPedestrianPhaseOverlap);

            var cycle = Assert.Single(result.Cycles);
            Assert.Equal(10, cycle.Delay);
        }

        [Theory]
        [InlineData(false, 21, 22)]
        [InlineData(true, 67, 68)]
        public void GetPedPhaseData_PreservesUnmatchedRecallWalk(
            bool isPedestrianPhaseOverlap,
            int beginWalkCode,
            int beginClearanceCode)
        {
            var walkTime = Start.AddSeconds(10);
            var events = CreateEvents(
                (10, beginWalkCode),
                (20, beginClearanceCode),
                (30, 90));

            var result = GetPedPhaseData(events, isPedestrianPhaseOverlap);

            Assert.Empty(result.Cycles);
            var recallWalk = Assert.Single(result.PedBeginWalkEvents);
            Assert.Equal(walkTime, recallWalk.Timestamp);
        }

        private static PedPhaseData GetPedPhaseData(
            List<IndianaEvent> events,
            bool isPedestrianPhaseOverlap,
            PedDelayOptions options = null)
        {
            options ??= CreateOptions();
            var approach = new Approach
            {
                Id = 1,
                ProtectedPhaseNumber = 2,
                IsPedestrianPhaseOverlap = isPedestrianPhaseOverlap,
                Location = new Location
                {
                    LocationIdentifier = LocationIdentifier
                }
            };

            return new PedPhaseService().GetPedPhaseData(
                options,
                approach,
                [],
                events);
        }

        private static PedDelayOptions CreateOptions()
        {
            return new PedDelayOptions
            {
                LocationIdentifier = LocationIdentifier,
                Start = Start,
                End = Start.AddMinutes(5),
                TimeBuffer = 15
            };
        }

        private static List<IndianaEvent> CreateEvents(params (double OffsetSeconds, int EventCode)[] events)
        {
            return events.Select(e => new IndianaEvent
            {
                LocationIdentifier = LocationIdentifier,
                Timestamp = Start.AddSeconds(e.OffsetSeconds),
                EventCode = (short)e.EventCode,
                EventParam = 2
            }).ToList();
        }
    }
}
