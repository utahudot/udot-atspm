#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Extensions/IndianaEventExtensionsTests.cs
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
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Extensions
{
    public class IndianaEventExtensionsTests
    {
        private const string Location = "LOC1";

        private IndianaEvent CreateEvent(string location, short phase, DateTime timestamp, short eventCode)
        {
            return new IndianaEvent
            {
                LocationIdentifier = location,
                EventParam = phase,
                EventCode = eventCode,
                Timestamp = timestamp
            };
        }

        #region IdentifyRedToRedCycles

        [Fact]
        public void ReturnsEmptyList_WhenNoEventsMatchSpecification()
        {
            var events = new List<IndianaEvent>(); // or events that fail FromSpecification

            var result = events.IdentifyRedToRedCycles();

            Assert.Empty(result);
        }

        [Fact]
        public void ReturnsEmptyList_WhenNoMatchingSequencesFound()
        {
            var now = DateTime.UtcNow;
            var events = new List<IndianaEvent>
        {
            CreateEvent(Location, 1, now.AddSeconds(0), (short)IndianaEnumerations.PhaseBeginGreen),
            CreateEvent(Location, 1, now.AddSeconds(1), (short)IndianaEnumerations.PhaseBeginYellowChange),
            CreateEvent(Location, 1, now.AddSeconds(2), (short)IndianaEnumerations.PhaseEndYellowChange),
            CreateEvent(Location, 1, now.AddSeconds(3), (short)IndianaEnumerations.PhaseBeginGreen) // breaks sequence
        };

            var result = events.IdentifyRedToRedCycles();

            Assert.Empty(result);
        }

        [Fact]
        public void DetectsSingleValidCycle_ForOnePhase()
        {
            var now = DateTime.UtcNow;
            var events = new List<IndianaEvent>
        {
            CreateEvent(Location, 1, now.AddSeconds(0), (short)IndianaEnumerations.PhaseEndYellowChange),
            CreateEvent(Location, 1, now.AddSeconds(1), (short)IndianaEnumerations.PhaseBeginGreen),
            CreateEvent(Location, 1, now.AddSeconds(2), (short)IndianaEnumerations.PhaseBeginYellowChange),
            CreateEvent(Location, 1, now.AddSeconds(3), (short)IndianaEnumerations.PhaseEndYellowChange)
        };

            var result = events.IdentifyRedToRedCycles();

            Assert.Single(result);
            var cycle = result.First();
            Assert.Equal(Location, cycle.LocationIdentifier);
            Assert.Equal(1, cycle.PhaseNumber);
            Assert.Equal(now.AddSeconds(0), cycle.Start);
            Assert.Equal(now.AddSeconds(1), cycle.GreenEvent);
            Assert.Equal(now.AddSeconds(2), cycle.YellowEvent);
            Assert.Equal(now.AddSeconds(3), cycle.End);
        }

        [Fact]
        public void DetectsMultipleCycles_AcrossMultiplePhases()
        {
            var now = DateTime.UtcNow;
            var events = new List<IndianaEvent>();

            for (short phase = 1; phase <= 8; phase++)
            {
                var offset = phase * 10;
                events.Add(CreateEvent(Location, phase, now.AddSeconds(offset + 0), (short)IndianaEnumerations.PhaseEndYellowChange));
                events.Add(CreateEvent(Location, phase, now.AddSeconds(offset + 1), (short)IndianaEnumerations.PhaseBeginGreen));
                events.Add(CreateEvent(Location, phase, now.AddSeconds(offset + 2), (short)IndianaEnumerations.PhaseBeginYellowChange));
                events.Add(CreateEvent(Location, phase, now.AddSeconds(offset + 3), (short)IndianaEnumerations.PhaseEndYellowChange));
            }

            var result = events.IdentifyRedToRedCycles();

            Assert.Equal(8, result.Count);
            foreach (var cycle in result)
            {
                Assert.Equal(Location, cycle.LocationIdentifier);
                Assert.InRange(cycle.PhaseNumber, 1, 8);
            }
        }

        [Fact]
        public void GroupsByLocationAndPhase_WhenMultipleLocationsPresent()
        {
            var now = DateTime.UtcNow;
            var events = new List<IndianaEvent>();

            for (int i = 0; i < 2; i++)
            {
                string loc = $"LOC{i + 1}";
                for (short phase = 1; phase <= 2; phase++)
                {
                    var offset = (i * 100) + (phase * 10);
                    events.Add(CreateEvent(loc, phase, now.AddSeconds(offset + 0), (short)IndianaEnumerations.PhaseEndYellowChange));
                    events.Add(CreateEvent(loc, phase, now.AddSeconds(offset + 1), (short)IndianaEnumerations.PhaseBeginGreen));
                    events.Add(CreateEvent(loc, phase, now.AddSeconds(offset + 2), (short)IndianaEnumerations.PhaseBeginYellowChange));
                    events.Add(CreateEvent(loc, phase, now.AddSeconds(offset + 3), (short)IndianaEnumerations.PhaseEndYellowChange));
                }
            }

            var result = events.IdentifyRedToRedCycles();

            Assert.Equal(4, result.Count);
            Assert.Contains(result, r => r.LocationIdentifier == "LOC1" && r.PhaseNumber == 1);
            Assert.Contains(result, r => r.LocationIdentifier == "LOC1" && r.PhaseNumber == 2);
            Assert.Contains(result, r => r.LocationIdentifier == "LOC2" && r.PhaseNumber == 1);
            Assert.Contains(result, r => r.LocationIdentifier == "LOC2" && r.PhaseNumber == 2);
        }

        [Fact]
        public void RespectsTimestampOrder_WhenEventsAreOutOfOrder()
        {
            var now = DateTime.UtcNow;
            var events = new List<IndianaEvent>
        {
            CreateEvent(Location, 1, now.AddSeconds(3), (short)IndianaEnumerations.PhaseEndYellowChange),
            CreateEvent(Location, 1, now.AddSeconds(0), (short)IndianaEnumerations.PhaseEndYellowChange),
            CreateEvent(Location, 1, now.AddSeconds(2), (short)IndianaEnumerations.PhaseBeginYellowChange),
            CreateEvent(Location, 1, now.AddSeconds(1), (short)IndianaEnumerations.PhaseBeginGreen)
        };

            var result = events.IdentifyRedToRedCycles();

            Assert.Single(result);
            var cycle = result.First();
            Assert.Equal(now.AddSeconds(0), cycle.Start);
            Assert.Equal(now.AddSeconds(1), cycle.GreenEvent);
            Assert.Equal(now.AddSeconds(2), cycle.YellowEvent);
            Assert.Equal(now.AddSeconds(3), cycle.End);
        }

        #endregion
    }
}
