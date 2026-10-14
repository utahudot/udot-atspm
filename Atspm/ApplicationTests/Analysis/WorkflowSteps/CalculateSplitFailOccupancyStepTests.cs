#region license
/// Copyright 2026 Utah Departement of Transportation
/// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/CalculateSplitFailOccupancyStepTests.cs
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
/// http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    /// <summary>
    /// Unit tests for the CalculateSplitFailOccupancyStep workflow step.
    /// </summary>
    public class CalculateSplitFailOccupancyStepTests : WorkflowStepTestBase<CalculateSplitFailOccupancyStep, CalculateSplitFailOccupancyTestData, Location, Tuple<Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>>, Tuple<Location, Dictionary<PhaseDetail, List<SplitFailCycleResult>>>>
    {
        /// <summary>
        /// Initializes a new instance of the CalculateSplitFailOccupancyStepTests class.
        /// </summary>
        /// <param name="output">The xUnit test output helper.</param>
        /// <param name="testLocationFixture">The test location class fixture.</param>
        public CalculateSplitFailOccupancyStepTests(ITestOutputHelper output, TestLocationFixture testLocationFixture) : base(output, testLocationFixture) { }

        /// <inheritdoc/>
        protected override Location DefaultTestConfig => TestLocation;

        /// <inheritdoc/>
        protected override Tuple<Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>> DefaultTestInput => Tuple.Create(new Dictionary<PhaseDetail, List<CycleTimestamps>>(), new Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>());

        /// <inheritdoc/>
        protected override CalculateSplitFailOccupancyStep CreateStep(Location config, Tuple<Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>> input, Tuple<Location, Dictionary<PhaseDetail, List<SplitFailCycleResult>>> expected)
        {
            return new CalculateSplitFailOccupancyStep();
        }

        /// <inheritdoc/>
        protected override Task<Tuple<Location, Dictionary<PhaseDetail, List<SplitFailCycleResult>>>> ExecuteStepAsync(CalculateSplitFailOccupancyStep step, Location config, Tuple<Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>> input, CancellationToken cancelToken = default)
        {
            return step.ExecuteAsync(Tuple.Create(config, input.Item1, input.Item2), cancelToken);
        }

        /// <summary>
        /// Verifies that an empty cycle and presence dictionary returns empty calculation results.
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateSplitFailOccupancyStep), "NoCycles")]
        public async Task Process_NoCycles_ReturnsEmptyResults()
        {
            var sut = new CalculateSplitFailOccupancyStep();
            var input = Tuple.Create(TestLocation, new Dictionary<PhaseDetail, List<CycleTimestamps>>(), new Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>());

            var result = await sut.ExecuteAsync(input);

            Assert.NotNull(result);
            Assert.Empty(result.Item2);
        }

        /// <summary>
        /// Verifies that 0% occupancy during Green and Red is calculated correctly and is not flagged as a split failure.
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateSplitFailOccupancyStep), "ClearApproach")]
        public async Task Process_ClearApproach_NoSplitFailure()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetail = CreateLocalPhaseDetail(localLocation, 2);

            var greenStart = DateTime.Today.AddHours(8);
            var cycles = new List<CycleTimestamps>
            {
                new()
                {
                    GreenStart = greenStart,
                    YellowStart = greenStart.AddSeconds(10),
                    RedStart = greenStart.AddSeconds(15),
                    GreenEnd = greenStart.AddSeconds(45)
                }
            };

            var presence = new List<Tuple<DateTime, DateTime>>();

            var cycleDict = new Dictionary<PhaseDetail, List<CycleTimestamps>> { { phaseDetail, cycles } };
            var presenceDict = new Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>> { { phaseDetail, presence } };

            var sut = new CalculateSplitFailOccupancyStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, cycleDict, presenceDict));

            var results = result.Item2[phaseDetail];
            Assert.Single(results);

            var r = results[0];
            Assert.Equal(0, r.GreenOccupancySeconds);
            Assert.Equal(0, r.RedOccupancySeconds);
            Assert.False(r.IsSplitFailure);
        }

        /// <summary>
        /// Verifies that green occupancy > 79% and red occupancy (first 5s) > 79% triggers IsSplitFailure = true.
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateSplitFailOccupancyStep), "HighGreenHighRedFailure")]
        public async Task Process_HighGreenHighRed_FlagsSplitFailure()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetail = CreateLocalPhaseDetail(localLocation, 2);

            var greenStart = DateTime.Today.AddHours(8);
            var cycles = new List<CycleTimestamps>
            {
                new()
                {
                    GreenStart = greenStart,
                    YellowStart = greenStart.AddSeconds(10),
                    RedStart = greenStart.AddSeconds(15),
                    GreenEnd = greenStart.AddSeconds(45)
                }
            };

            var presence = new List<Tuple<DateTime, DateTime>>
            {
                Tuple.Create(greenStart.AddSeconds(1), greenStart.AddSeconds(10)),
                Tuple.Create(greenStart.AddSeconds(15), greenStart.AddSeconds(20))
            };

            var cycleDict = new Dictionary<PhaseDetail, List<CycleTimestamps>> { { phaseDetail, cycles } };
            var presenceDict = new Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>> { { phaseDetail, presence } };

            var sut = new CalculateSplitFailOccupancyStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, cycleDict, presenceDict));

            var results = result.Item2[phaseDetail];
            Assert.Single(results);

            var r = results[0];
            Assert.Equal(9.0, r.GreenOccupancySeconds);
            Assert.Equal(5.0, r.RedOccupancySeconds);
            Assert.True(r.IsSplitFailure);
        }

        /// <summary>
        /// Verifies that high green occupancy combined with low red occupancy is not flagged as a split failure.
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateSplitFailOccupancyStep), "HighGreenLowRedNoFailure")]
        public async Task Process_HighGreenLowRed_NoSplitFailure()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetail = CreateLocalPhaseDetail(localLocation, 2);

            var greenStart = DateTime.Today.AddHours(8);
            var cycles = new List<CycleTimestamps>
            {
                new()
                {
                    GreenStart = greenStart,
                    YellowStart = greenStart.AddSeconds(10),
                    RedStart = greenStart.AddSeconds(15),
                    GreenEnd = greenStart.AddSeconds(45)
                }
            };

            var presence = new List<Tuple<DateTime, DateTime>>
            {
                Tuple.Create(greenStart.AddSeconds(1), greenStart.AddSeconds(10)),
                Tuple.Create(greenStart.AddSeconds(15), greenStart.AddSeconds(16))
            };

            var cycleDict = new Dictionary<PhaseDetail, List<CycleTimestamps>> { { phaseDetail, cycles } };
            var presenceDict = new Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>> { { phaseDetail, presence } };

            var sut = new CalculateSplitFailOccupancyStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, cycleDict, presenceDict));

            var results = result.Item2[phaseDetail];
            Assert.Single(results);

            var r = results[0];
            Assert.Equal(9.0, r.GreenOccupancySeconds);
            Assert.Equal(1.0, r.RedOccupancySeconds);
            Assert.False(r.IsSplitFailure);
        }

        /// <summary>
        /// Verifies that detector presence lasting beyond 5 seconds is only calculated for the exact configured red analysis seconds.
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateSplitFailOccupancyStep), "RedWindowConstraint")]
        public async Task Process_RedWindowConstraint_RespectsRedAnalysisSeconds()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetail = CreateLocalPhaseDetail(localLocation, 2);

            var greenStart = DateTime.Today.AddHours(8);
            var cycles = new List<CycleTimestamps>
            {
                new()
                {
                    GreenStart = greenStart,
                    YellowStart = greenStart.AddSeconds(10),
                    RedStart = greenStart.AddSeconds(15),
                    GreenEnd = greenStart.AddSeconds(45)
                }
            };

            var presence = new List<Tuple<DateTime, DateTime>>
            {
                Tuple.Create(greenStart.AddSeconds(15), greenStart.AddSeconds(45))
            };

            var cycleDict = new Dictionary<PhaseDetail, List<CycleTimestamps>> { { phaseDetail, cycles } };
            var presenceDict = new Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>> { { phaseDetail, presence } };

            var sut = new CalculateSplitFailOccupancyStep(redAnalysisSeconds: 5.0);
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, cycleDict, presenceDict));

            var r = result.Item2[phaseDetail][0];
            Assert.Equal(5.0, r.RedOccupancySeconds);
        }

        /// <summary>
        /// Verifies that zero-duration green intervals (which can occur during preemption overrides) do not cause division-by-zero crashes.
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateSplitFailOccupancyStep), "ZeroDurationGreen")]
        public async Task Process_ZeroDurationGreen_AvoidsDivisionByZeroCrash()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetail = CreateLocalPhaseDetail(localLocation, 2);

            var greenStart = DateTime.Today.AddHours(8);
            var cycles = new List<CycleTimestamps>
            {
                new()
                {
                    GreenStart = greenStart,
                    YellowStart = greenStart,
                    RedStart = greenStart.AddSeconds(5),
                    GreenEnd = greenStart.AddSeconds(15)
                }
            };

            var presence = new List<Tuple<DateTime, DateTime>>();

            var cycleDict = new Dictionary<PhaseDetail, List<CycleTimestamps>> { { phaseDetail, cycles } };
            var presenceDict = new Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>> { { phaseDetail, presence } };

            var sut = new CalculateSplitFailOccupancyStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, cycleDict, presenceDict));

            var r = result.Item2[phaseDetail][0];
            Assert.Equal(0, r.GreenOccupancySeconds);
            Assert.False(r.IsSplitFailure);
        }

        private Location CreateLocalMockLocation()
        {
            return new Location { LocationIdentifier = "MOCK_7115" };
        }

        private PhaseDetail CreateLocalPhaseDetail(Location location, int phaseNum)
        {
            var approach = new Approach
            {
                Id = phaseNum,
                ProtectedPhaseNumber = phaseNum,
                IsPedestrianPhaseOverlap = false,
                Mph = 35
            };
            location.Approaches.Add(approach);

            return new PhaseDetail
            {
                Approach = approach,
                UseOverlap = false,
                PhaseNumber = approach.ProtectedPhaseNumber
            };
        }
    }
}
