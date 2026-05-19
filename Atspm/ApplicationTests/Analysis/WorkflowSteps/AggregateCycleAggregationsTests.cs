#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/AggregateCycleAggregationsTests.cs
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
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class AggregateCycleAggregationsTests : IDisposable
    {
        private static List<IndianaEvent> MakeRedToRedCycle(DateTime t0)
        {
            return new List<IndianaEvent>
            {
                new IndianaEvent { EventCode = 9, Timestamp = t0 },                 // ChangeToRed
                new IndianaEvent { EventCode = 1, Timestamp = t0.AddSeconds(5) },   // ChangeToGreen
                new IndianaEvent { EventCode = 8, Timestamp = t0.AddSeconds(10) },  // ChangeToYellow
                new IndianaEvent { EventCode = 9, Timestamp = t0.AddSeconds(15) }   // ChangeToRed
            };
        }

        private static List<IndianaEvent> MakeGreenToGreenCycle(DateTime t0)
        {
            return new List<IndianaEvent>
            {
                new IndianaEvent { EventCode = 1, Timestamp = t0 },                 // Green
                new IndianaEvent { EventCode = 8, Timestamp = t0.AddSeconds(5) },   // Yellow
                new IndianaEvent { EventCode = 9, Timestamp = t0.AddSeconds(10) },  // Red
                new IndianaEvent { EventCode = 1, Timestamp = t0.AddSeconds(15) }   // Green
            };
        }

        [Fact]
        [Trait(nameof(AggregateCycleAggregations), "RedToRed")]
        public void GetRedToRedCycles_ShouldReturnCycle()
        {
            var sut = new AggregateCycleAggregations(TimeSpan.FromMinutes(15));
            var start = DateTime.Parse("2023-04-17T00:00:00Z");
            var events = MakeRedToRedCycle(start);

            var cycles = sut.GetRedToRedCycles(start, start.AddMinutes(1), events);

            Assert.Collection(cycles, c =>
            {
                Assert.Equal(start, c.StartTime);
                Assert.Equal(start.AddSeconds(15), c.EndTime);
                Assert.Equal(5, (int)Math.Round(c.TotalRedTimeSeconds));
                Assert.Equal(5, (int)Math.Round(c.TotalGreenTimeSeconds));
                Assert.Equal(5, (int)Math.Round(c.TotalYellowTimeSeconds));
            });
        }

        [Fact]
        [Trait(nameof(AggregateCycleAggregations), "GreenToGreen")]
        public void GetGreenToGreenCycles_ShouldReturnCycle()
        {
            var sut = new AggregateCycleAggregations(TimeSpan.FromMinutes(15));
            var start = DateTime.Parse("2023-04-17T00:00:00Z");
            var events = MakeGreenToGreenCycle(start);

            var cycles = sut.GetGreenToGreenCycles(start, start.AddMinutes(1), events);

            Assert.Collection(cycles, c =>
            {
                Assert.Equal(start, c.StartTime);
                Assert.Equal(start.AddSeconds(15), c.EndTime);
            });
        }

        [Fact]
        [Trait(nameof(AggregateCycleAggregations), "Empty")]
        public void GetRedToRedCycles_ShouldReturnEmpty_WhenNoValidPattern()
        {
            var sut = new AggregateCycleAggregations(TimeSpan.FromMinutes(15));
            var start = DateTime.Parse("2023-04-17T00:00:00Z");
            var events = new List<IndianaEvent>
            {
                new IndianaEvent { EventCode = 1, Timestamp = start },  // Green
                new IndianaEvent { EventCode = 8, Timestamp = start.AddSeconds(5) } // Yellow
            };

            var cycles = sut.GetRedToRedCycles(start, start.AddMinutes(1), events);

            Assert.Empty(cycles);
        }

        [Fact]
        [Trait(nameof(AggregateCycleAggregations), "WindowFilter")]
        public void Cycles_ShouldRespectWindow()
        {
            var sut = new AggregateCycleAggregations(TimeSpan.FromMinutes(15));
            var start = DateTime.Parse("2023-04-17T00:00:00Z");
            var events = MakeRedToRedCycle(start);

            var cycles = sut.GetRedToRedCycles(start.AddMinutes(1), start.AddMinutes(2), events);

            Assert.Empty(cycles);
        }

        public void Dispose()
        {
            // cleanup if needed
        }
    }
}
