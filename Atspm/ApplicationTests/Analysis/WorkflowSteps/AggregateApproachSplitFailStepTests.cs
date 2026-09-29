#region license
/// Copyright 2026 Utah Departement of Transportation
/// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/AggregateApproachSplitFailStepTests.cs
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
using Utah.Udot.Atspm.TempExtensions;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    /// <summary>
    /// Unit tests for the AggregateApproachSplitFailStep workflow step.
    /// </summary>
    public class AggregateApproachSplitFailStepTests : WorkflowStepTestBase<AggregateApproachSplitFailStep, AggregateApproachSplitFailTestData, Location, Dictionary<PhaseDetail, List<SplitFailCycleResult>>, IEnumerable<ApproachSplitFailAggregation>>
    {
        /// <summary>
        /// Initializes a new instance of the AggregateApproachSplitFailStepTests class.
        /// </summary>
        /// <param name="output">The xUnit test output helper.</param>
        /// <param name="testLocationFixture">The test location class fixture.</param>
        public AggregateApproachSplitFailStepTests(ITestOutputHelper output, TestLocationFixture testLocationFixture) : base(output, testLocationFixture) { }

        /// <inheritdoc/>
        protected override Location DefaultTestConfig => TestLocation;

        /// <inheritdoc/>
        protected override Dictionary<PhaseDetail, List<SplitFailCycleResult>> DefaultTestInput => new Dictionary<PhaseDetail, List<SplitFailCycleResult>>();

        /// <inheritdoc/>
        protected override AggregateApproachSplitFailStep CreateStep(Location config, Dictionary<PhaseDetail, List<SplitFailCycleResult>> input, IEnumerable<ApproachSplitFailAggregation> expected)
        {
            var baseline = DateTime.Today;

            foreach (var results in input.Values)
            {
                if (results.Count > 0)
                {
                    baseline = results[0].GreenStart.Date;
                    break;
                }
            }

            var timeline = baseline.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));
            return new AggregateApproachSplitFailStep(timeline);
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<ApproachSplitFailAggregation>> ExecuteStepAsync(AggregateApproachSplitFailStep step, Location config, Dictionary<PhaseDetail, List<SplitFailCycleResult>> input, CancellationToken cancelToken = default)
        {
            return step.ExecuteAsync(Tuple.Create(config, input), cancelToken);
        }

        /// <summary>
        /// Verifies that empty inputs return empty aggregations if no timeline segments or phases exist, or zero-filled segments if they do.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateApproachSplitFailStep), "EmptyInput")]
        public async Task Process_EmptyInput_ReturnsCorrectAggregations()
        {
            var timeline = DateTime.Today.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));
            var sut = new AggregateApproachSplitFailStep(timeline);
            var input = Tuple.Create(TestLocation, new Dictionary<PhaseDetail, List<SplitFailCycleResult>>());

            var result = await sut.ExecuteAsync(input);

            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that calculated cycle results are correctly binned into their appropriate 15-minute timeline segments.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateApproachSplitFailStep), "TimelineBinning")]
        public async Task Process_TimelineSegments_GroupsBinnedDataCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetail = CreateLocalPhaseDetail(localLocation, 2);

            var start = DateTime.Today.AddHours(8);
            var cycles = new List<SplitFailCycleResult>
            {
                new()
                {
                    GreenStart = start.AddMinutes(2),
                    YellowStart = start.AddMinutes(2).AddSeconds(10),
                    RedStart = start.AddMinutes(2).AddSeconds(15),
                    GreenEnd = start.AddMinutes(2).AddSeconds(45),
                    GreenOccupancySeconds = 10,
                    RedOccupancySeconds = 4,
                    IsSplitFailure = true
                },
                new()
                {
                    GreenStart = start.AddMinutes(17),
                    YellowStart = start.AddMinutes(17).AddSeconds(10),
                    RedStart = start.AddMinutes(17).AddSeconds(15),
                    GreenEnd = start.AddMinutes(17).AddSeconds(45),
                    GreenOccupancySeconds = 15,
                    RedOccupancySeconds = 1,
                    IsSplitFailure = false
                }
            };

            var input = new Dictionary<PhaseDetail, List<SplitFailCycleResult>> { { phaseDetail, cycles } };

            var timeline = start.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));
            var sut = new AggregateApproachSplitFailStep(timeline);
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, input));

            var list = result.ToList();
            Assert.NotEmpty(list);

            var bin1 = list.FirstOrDefault(a => a.Start == start);
            var bin2 = list.FirstOrDefault(a => a.Start == start.AddMinutes(15));

            Assert.NotNull(bin1);
            Assert.Equal(1, bin1.Cycles);
            Assert.Equal(1, bin1.SplitFailures);

            Assert.NotNull(bin2);
            Assert.Equal(1, bin2.Cycles);
            Assert.Equal(0, bin2.SplitFailures);
        }

        /// <summary>
        /// Verifies that cycles falling outside the start and end of the timeline segments are omitted.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateApproachSplitFailStep), "BoundaryCycles")]
        public async Task Process_BoundaryCycles_AreExcluded()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetail = CreateLocalPhaseDetail(localLocation, 2);

            var start = DateTime.Today.AddHours(8);
            var cycles = new List<SplitFailCycleResult>
            {
                new()
                {
                    GreenStart = start.AddMinutes(-5),
                    YellowStart = start.AddMinutes(-5).AddSeconds(10),
                    RedStart = start.AddMinutes(-5).AddSeconds(15),
                    GreenEnd = start.AddMinutes(-5).AddSeconds(45),
                    GreenOccupancySeconds = 5,
                    RedOccupancySeconds = 1,
                    IsSplitFailure = false
                }
            };

            var input = new Dictionary<PhaseDetail, List<SplitFailCycleResult>> { { phaseDetail, cycles } };

            var timeline = start.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));
            var sut = new AggregateApproachSplitFailStep(timeline);
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, input));

            var list = result.ToList();
            var outsideBin = list.FirstOrDefault(a => a.Start == start);

            Assert.NotNull(outsideBin);
            Assert.Equal(0, outsideBin.Cycles);
        }

        /// <summary>
        /// Verifies that segments with no cycles are zero-filled rather than omitted to maintain data continuity.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateApproachSplitFailStep), "ZeroFillAggregations")]
        public async Task Process_NoCycles_CreatesZeroFillAggregation()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetail = CreateLocalPhaseDetail(localLocation, 2);

            var start = DateTime.Today.AddHours(8);
            var input = new Dictionary<PhaseDetail, List<SplitFailCycleResult>> { { phaseDetail, new List<SplitFailCycleResult>() } };

            var timeline = start.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));
            var sut = new AggregateApproachSplitFailStep(timeline);
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, input));

            var list = result.ToList();
            Assert.NotEmpty(list);

            var bin1 = list.FirstOrDefault(a => a.Start == start);
            Assert.NotNull(bin1);
            Assert.Equal(0, bin1.Cycles);
            Assert.Equal(0, bin1.SplitFailures);
            Assert.Equal(0, bin1.GreenOccupancySum);
            Assert.Equal(0, bin1.RedOccupancySum);
            Assert.Equal(0, bin1.RedTimeSum);
        }

        /// <summary>
        /// Verifies that permissive and protected phase configurations are accurately mapped to the IsProtectedPhase column.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateApproachSplitFailStep), "PhaseMapping")]
        public async Task Process_ProtectedPermissivePhases_MapsBooleansCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var phaseDetailProt = CreateLocalPhaseDetail(localLocation, 2, isPermissive: false);
            var phaseDetailPerm = CreateLocalPhaseDetail(localLocation, 3, isPermissive: true);

            var start = DateTime.Today.AddHours(8);
            var input = new Dictionary<PhaseDetail, List<SplitFailCycleResult>>
            {
                { phaseDetailProt, new List<SplitFailCycleResult>() },
                { phaseDetailPerm, new List<SplitFailCycleResult>() }
            };

            var timeline = start.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));
            var sut = new AggregateApproachSplitFailStep(timeline);
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, input));

            var list = result.ToList();

            var protAgg = list.FirstOrDefault(a => a.PhaseNumber == 2 && a.Start == start);
            var permAgg = list.FirstOrDefault(a => a.PhaseNumber == 3 && a.Start == start);

            Assert.NotNull(protAgg);
            Assert.True(protAgg.IsProtectedPhase);

            Assert.NotNull(permAgg);
            Assert.False(permAgg.IsProtectedPhase);
        }

        private Location CreateLocalMockLocation()
        {
            return new Location { LocationIdentifier = "MOCK_7115" };
        }

        private PhaseDetail CreateLocalPhaseDetail(Location location, int phaseNum, bool isPermissive = false)
        {
            var approach = new Approach
            {
                Id = phaseNum,
                ProtectedPhaseNumber = phaseNum,
                IsPedestrianPhaseOverlap = false,
                Mph = 35
            };
            location.Approaches.Add(approach);

            return new PhaseDetail { Approach = approach, UseOverlap = false, PhaseNumber = approach.ProtectedPhaseNumber, IsPermissivePhase = isPermissive };
        }
    }
}
